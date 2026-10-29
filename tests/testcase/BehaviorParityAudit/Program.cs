// Diagnostic only: CONFIRMED means a discrepancy was observed, not correctness.
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Shooting;
using SlimDX.XAudio2;
using SlimDX.Multimedia;
using Silk.NET.OpenAL;
using Device=SlimDX.Direct3D9.Device;

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
string temp=Path.Combine(Path.GetTempPath(),"thsss-behavior-audit-"+Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(temp);
int differences=0;
void Observe(bool discrepancy,string message)
{
    Console.WriteLine((discrepancy?"CONFIRMED: ":"NOT REPRODUCED: ")+message);
    if(discrepancy) differences++;
}
try
{
    var values=new List<int>{1,2}; var visited=new List<int>();
    foreach(int item in SlimDX.Compat.ListCompat.Snapshot(values))
    { visited.Add(item); if(item==1) values.Remove(2); }
    Observe(visited.SequenceEqual(new[]{1,2}),"snapshot still executes removed item: visited="+string.Join(',',visited)+" (contradicts shim comment 'removed entries skipped')");
    values=new List<int>{1,2}; visited.Clear();
    foreach(int item in SlimDX.Compat.ListCompat.Snapshot(values))
    { visited.Add(item); if(item==1) values.Add(3); }
    Observe(!visited.Contains(3),"snapshot defers appended entry: visited="+string.Join(',',visited)+"; live index iteration would include appended 3");
    var eventObject=new BaseObject_CS{EventGroupList=new List<EventGroup>(),EventsExecutionList=new List<Execution>()};
    var expired=new Execution{changevalue=999,ctime=1};
    var next=new Execution{changevalue=999,ctime=2};
    eventObject.EventsExecutionList.AddRange(new[]{expired,next});
    eventObject.EventCtrl();
    Observe(next.ctime==1,"actual BaseObject_CS event after self-removal decrements next event in same frame; live-index model would leave ctime=2 (original CLR verification still needed)");
    using(var font=new SlimDX.Direct3D9.Font(new Device(),new System.Drawing.Font("Arial",20)))
    {
        int width=font.MeasureString("很长的中文剧情对白ABCDEFGHIJKLMNOPQRSTUVWXYZ",SlimDX.Direct3D9.DrawTextFormat.Left).Width;
        Observe(width==0,"nonempty dialogue measures width="+width+"; wrapping conditions never trigger");
    }
    string truncated=Path.Combine(temp,"truncated.rpy"); File.WriteAllBytes(truncated,Array.Empty<byte>());
    var replay=new Replay(); replay.LoadRpy(truncated); int key=replay.ReadKey(); replay.Dispose();
    Observe(key==-257,"truncated replay EOF becomes key="+key+", not end marker 4080 or explicit error");
    string title=Path.Combine(temp,"culture.rpy");
    File.WriteAllLines(title,new[]{"","ReplayInformation","ver 1.00","Test","26/10/20","12:00","Reimu","A","1","St1","St1","0.0", "2\t3\t100\t0\t0\t192.5\t400.5\t0\t0\t0\t0\t1000\t1.25\t0\t0"});
    var culture=CultureInfo.CurrentCulture;
    try
    {
        CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("en-US"); var en=Replay.ReadTitle(title);
        CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo("de-DE"); var de=Replay.ReadTitle(title);
        float xEn=en.MyPlaneData[0].PosX, xDe=de.MyPlaneData[0].PosX;
        Observe(xEn!=xDe,"same replay position en-US="+xEn.ToString(CultureInfo.InvariantCulture)+" de-DE="+xDe.ToString(CultureInfo.InvariantCulture));
    }
    finally { CultureInfo.CurrentCulture=culture; }
    const string legacyName="博丽灵梦";
    using(var des=DES.Create())
    {
        des.Key=Encoding.ASCII.GetBytes("SeiweLL0"); des.IV=Encoding.ASCII.GetBytes("0SeiweLL");
        byte[] plain=Encoding.GetEncoding(936).GetBytes(legacyName);
        byte[] cipher=des.CreateEncryptor().TransformFinalBlock(plain,0,plain.Length);
        var encryption=typeof(Replay).Assembly.GetType("Shooting.Encryption",true)!;
        var decoded=(string)encryption.GetMethod("Decrypt",BindingFlags.Public|BindingFlags.Static)!.Invoke(null,new object[]{Convert.ToHexString(cipher)})!;
        Observe(decoded!=legacyName,"CP936 legacy DES name is not preserved by current decoder: "+decoded);
    }
    string oldCwd=Directory.GetCurrentDirectory();
    try
    {
        Directory.SetCurrentDirectory(temp);
        const string corrupt="audit-corrupt-score-fixture";
        File.WriteAllText("Score.dat",corrupt);
        var player=new PlayerData();
        player.SaveData(); // Same automatic save invoked by MainMenu.Init after load.
        Observe(File.ReadAllText("Score.dat")!=corrupt,"failed Score load followed by main-menu SaveData overwrites the damaged original (isolated temp fixture)");
    }
    finally { Directory.SetCurrentDirectory(oldCwd); }
    // All audio is silence. Hardware/backend availability is reported separately.
    using var engine=new XAudio2();
    var format=new WaveFormat(22050,16,1);
    using(var voice=new SourceVoice(engine,format))
    {
        voice.Volume=0.2f; voice.SetOutputMatrix(1,2,new[]{1f,0f});
        float actual=voice.Volume;
        Observe(Math.Abs(actual-0.2f)>0.001,"mono pan matrix changes gain from 0.2 to "+actual.ToString(CultureInfo.InvariantCulture));
    }
    using var al=AL.GetApi();
    byte[] Wav(double seconds)
    {
        using var data=new MemoryStream(); using var w=new BinaryWriter(data,Encoding.ASCII,true);
        int bytes=(int)(22050*seconds)*2;
        w.Write(Encoding.ASCII.GetBytes("RIFF"));w.Write(36+bytes);w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
        w.Write(16);w.Write((short)1);w.Write((short)1);w.Write(22050);w.Write(44100);w.Write((short)2);w.Write((short)16);
        w.Write(Encoding.ASCII.GetBytes("data"));w.Write(bytes);w.Write(new byte[bytes]);w.Flush(); return data.ToArray();
    }
    uint Source(SourceVoice voice)=>(uint)typeof(SourceVoice).GetField("_source",BindingFlags.NonPublic|BindingFlags.Instance)!.GetValue(voice)!;
    using(var stream=new WaveStream(new MemoryStream(Wav(0.15))))
    using(var voice=new SourceVoice(engine,stream.Format))
    {
        voice.SubmitSourceBuffer(new AudioBuffer{AudioData=stream,LoopCount=255});
        uint source=Source(voice);
        if(source==0) Console.WriteLine("UNVERIFIED: OpenAL backend unavailable, looping/resume probes skipped");
        else
        {
            voice.Start(); Thread.Sleep(450);
            al.GetSourceProperty(source,GetSourceInteger.SourceState,out int state);
            Observe(state==0x1014,"requested infinite-loop 0.15s buffer stopped after 0.45s: AL state="+state);
        }
    }
    using(var stream=new WaveStream(new MemoryStream(Wav(2))))
    using(var voice=new SourceVoice(engine,stream.Format))
    {
        voice.SubmitSourceBuffer(new AudioBuffer{AudioData=stream}); uint source=Source(voice);
        if(source!=0)
        {
            voice.Start(); Thread.Sleep(200); al.GetSourceProperty(source,GetSourceInteger.SampleOffset,out int before);
            voice.Stop(); voice.Start(); al.GetSourceProperty(source,GetSourceInteger.SampleOffset,out int after);
            Observe(before>1000 && after<before/2,"Stop/Start rewinds OpenAL cursor: before="+before+" after="+after);
        }
    }
    if(args.Contains("--gpu")) differences+=GpuParityProbes.Run(temp);
    Console.WriteLine($"DIAGNOSTIC COMPLETE: {differences} observed discrepancies; not a parity PASS.");
}
finally { Directory.Delete(temp,true); }
