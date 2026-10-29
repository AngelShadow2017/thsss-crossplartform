#if !defined(__x86_64__)
#error Metal API shim trampoline supports only Linux x86_64 SysV ABI
#endif
.text
.globl objc_msgSend
.type objc_msgSend,@function
objc_msgSend:
    # Preserve every argument register and leave stack-passed structs untouched.
    push %rdi
    push %rsi
    push %rdx
    push %rcx
    push %r8
    push %r9
    push %rax
    sub $128, %rsp
    movdqu %xmm0, 0(%rsp)
    movdqu %xmm1, 16(%rsp)
    movdqu %xmm2, 32(%rsp)
    movdqu %xmm3, 48(%rsp)
    movdqu %xmm4, 64(%rsp)
    movdqu %xmm5, 80(%rsp)
    movdqu %xmm6, 96(%rsp)
    movdqu %xmm7, 112(%rsp)
    mov %rsi, %rdi
    call shim_dispatch@PLT
    mov %rax, %r11
    movdqu 0(%rsp), %xmm0
    movdqu 16(%rsp), %xmm1
    movdqu 32(%rsp), %xmm2
    movdqu 48(%rsp), %xmm3
    movdqu 64(%rsp), %xmm4
    movdqu 80(%rsp), %xmm5
    movdqu 96(%rsp), %xmm6
    movdqu 112(%rsp), %xmm7
    add $128, %rsp
    pop %rax
    pop %r9
    pop %r8
    pop %rcx
    pop %rdx
    pop %rsi
    pop %rdi
    jmp *%r11
.size objc_msgSend,.-objc_msgSend
.section .note.GNU-stack,"",@progbits
