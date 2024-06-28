Emulator for Subleqx machine.

Subleqx is an extension of Subleq OISC machine, which allows arbitrary widths for pointers and data.  

Instructions are encoded as sequence of bits and executed within linear block of memory.  

In theory, binary format of Subleqx allows to address infinite amount of RAM, making this machine Turing-complete.
However, for emulation on real hardware, amount of available RAM needs to be limited.

Subleqx project can be split into 4 parts:
1. Binary format of instructions;
2. Memory-mapped input and output;
3. Assembly language ([SubleqxAsm](https://github.com/Vort/SubleqxAsm) repository);
4. Emulation (this repository).

Each Subleqx instruction consists of 6 parts:
- Address width (`aw`);
- Data width (`dwm1w` and `dwm1`);
- Pointers A, B and C (`pa`, `pb` and `pc`).

For program to interact with user, three special addresses are defined:
- Reading of address `1` halts execution (emulation stops);
- Reading of address `2` reads UTF-32 character from console;
- Writing to address `3` writes UTF-32 character to console.

Assembly language allows to define instructions and variables. Here is how "Hello, world!" program can be implemented with it:
```
loop:
  8, 3, 6, ptr: msg, 3, @n
  7, 3, 7, cm7, ptr, @n
  7, 3, 4, cm1, i, loop

  1, 1, 0, 1, 1, 0

i:   .d5 1 - 13
cm1: .d5 -1
cm7: .d8 -7
msg: .s7 "Hello, world!"
```

To run this program, these commands should be executed:
- `SubleqxAsm hello.sxa`
- `SubleqxEmu hello.bin`

Subleqx project is work in progress, I plan to improve it, however, binary format should remain most stable, with main changes focusing in assembly and emulation parts.