# forth-memory-management
Pair an array of potential struct instances, with a special-purpose stack, giving an array-stack.

Multiple array-stacks can be created, with the number of items, and the size of each item, varying by array-stack as needed.

The stack of an array-stack is initialized with the address of each array item.

Allocation and deallocation is fairly fast because it involves simply popping, or pushing, the stack.

Within the limit of the maximum number of array items allocated at the same time,
an infinite number of allocations, and deallocations, are possible.

The files can be brought into gforth with the command: include example.fs 

On deallocation, the first cell of an item is zeroed out, to make use problems apparent.

Allocations, and deallocations, causes increasing disorder of the addresses on the stack,
which has no effect on the utility, or speed, of the array-stack.

That a stack is used to provide memory management to a stack-based language is sublime.

The examples use linked lists of numbers, and strings.

Functions that manipulate a struct instance can act as wrappers to array-stack, and list, functions.

Diagnosis of a memory leak can begin with the array-stack that becomes exhausted.

The first word of every struct instance, allocated from the same array-stack, can be set to a unique number, to indicate the type of struct.

These ideas should work in Assembler Language and C.
