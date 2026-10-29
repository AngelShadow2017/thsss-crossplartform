using SlimDX.Compat;
using Shooting;
int checks=0;
void Equal<T>(IEnumerable<T> actual,IEnumerable<T> expected,string name)
{
    if(!actual.SequenceEqual(expected)) throw new Exception(name+": "+string.Join(',',actual));
    checks++;
}
void Reference(List<int> values,Action<int> action)
{
    // Pre-4.5 List.ForEach: current _size is tested every turn, _items[index] passed to action.
    for(int index=0;index<values.Count;index++) action(values[index]);
}
foreach(int scenario in Enumerable.Range(0,5))
{
    List<int> Run(bool reference)
    {
        var values=new List<int>{1,2,3};var seen=new List<int>();
        void Visit(int item)
        {
            seen.Add(item);
            if(item!=1) return;
            switch(scenario)
            {
                case 0: values.Remove(1);break;
                case 1: values.Remove(2);break;
                case 2: values.Add(4);break;
                case 3: values.Clear();break;
                case 4: values[1]=9;break;
            }
        }
        if(reference) Reference(values,Visit);
        else foreach(int item in ListCompat.LegacyItems(values)) Visit(item);
        return seen;
    }
    Equal(Run(false),Run(true),"legacy mutation scenario="+scenario);
}
var list=new List<int>{1,2};var visited=new List<int>();
ListCompat.ForEach(list,item=>{visited.Add(item);if(item==1)list.Add(3);});
Equal(visited,new[]{1,2,3},"legacy callback append same pass");
var obj=new BaseObject_CS{EventGroupList=new List<EventGroup>(),EventsExecutionList=new List<Execution>()};
var expiring=new Execution{changevalue=999,ctime=1};var next=new Execution{changevalue=999,ctime=2};
obj.EventsExecutionList.AddRange(new[]{expiring,next});obj.EventCtrl();
if(next.ctime!=2||obj.EventsExecutionList.Count!=1)throw new Exception("Actual event self-removal did not preserve original live-index shift");checks++;
obj.EventCtrl();if(next.ctime!=1)throw new Exception("Remaining event did not run on next frame");checks++;
Console.WriteLine($"PASS: {checks} legacy enumeration and actual event pipeline checks");

using(var release=new ManualResetEventSlim())
{
    var worker=new Thread(()=>release.Wait()){IsBackground=true,Name="list-test-worker"};worker.Start();
    bool timedOut=false;
    try { ThreadCompat.Stop(worker,5); } catch(TimeoutException) { timedOut=true; }
    finally { release.Set(); worker.Join(); }
    if(!timedOut)throw new Exception("Unfinished thread was reported as stopped");checks++;
    ThreadCompat.Stop(worker);
}
Console.WriteLine("PASS: unfinished worker fails explicitly; completed worker joins safely");

var background=new BackgroundManager();
var first=new AuditObject{Outside=true};var shifted=new AuditObject{Outside=true};var last=new AuditObject();
background.BackgroundList.AddRange(new BaseObject[]{first,shifted,last});background.Ctrl();
if(background.BackgroundList.Count!=2||!background.BackgroundList.Contains(shifted)||shifted.Updates!=1)
    throw new Exception("Background removal did not retain legacy shifted-index behavior");
var spawning=new AuditObject();var spawned=new AuditObject();
background.BackgroundList.Clear();background.BackgroundList.Add(spawning);
spawning.OnUpdate=()=>{if(!background.BackgroundList.Contains(spawned))background.BackgroundList.Add(spawned);};
background.Ctrl();if(spawned.Updates!=1)throw new Exception("Appended background was deferred instead of controlled in same legacy pass");
Console.WriteLine("PASS: actual background removal and same-pass spawn ordering");

sealed class AuditObject:BaseObject
{
    public bool Outside;public int Updates;public Action OnUpdate;
    public override bool OutBoundary()=>Outside;
    public override void Ctrl(){Updates++;OnUpdate?.Invoke();}
}
