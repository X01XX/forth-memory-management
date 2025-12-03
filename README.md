# forth-memory-management
Pair an array of potential struct instances, with a special-purpose stack, giving an array-stack.

Multiple array-stacks can be created, with the number of items, and the size of each item, varying by array-stack as needed.

The stack of an array-stack is initialized with the address of each array item.

Allocation and deallocation is fairly fast because it involves simply popping, or pushing, the stack.

Within the limit of the maximum number of array items allocated at the same time,
an infinite number of allocations, and deallocations, are possible.

On deallocation, the first cell of an item is zeroed out, to make use problems apparent.

Allocations, and deallocations, causes increasing disorder of the addresses on the stack,
which has no effect on the utility, or speed, of the array-stack.

That a stack is used to provide memory management to a stack-based language is sublime.

Functions that manipulate a struct instance can act as wrappers to array-stack, and list, functions.

Diagnosis of a memory leak can begin with the array-stack that becomes exhausted.

The first word of every struct instance, allocated from the same array-stack, can be set to a unique number, to indicate the type of struct.

These ideas should work in Assembler Language and C (if you want faster allocation/deallocation, more control).

Lists are built of List structs, and Link structs, that have an ID and use-count in their header.

The examples can be run with the commands:
<pre>
  gforth example.fs -e bye    \ Shows two lists of numbers, with set functions union, intersection and subtraction.
                              \ Shows applying multiplication, and addition, of all list elements, given a number.
                              \ The numbers are not structs, just numbers in the Link data field.
                              \ Shows two lists of names, with set functions union, intersection and subtraction. The Name is a struct.

  gforth example2.fs -e bye   \ Shows my favorite calculation ~A + ~B, with Karnaugh Map regions. The Region is a struct.
                              \ ~A + ~B forms regions with A, regions with B, regions with neither, NO regions with A and B.

  gforth example3.fs -e bye   \ Shows lists of lists of regions.

  gforth example4.fs -e bye   \ Shows a struct-aware print of items on a stack, for debugging.
</pre>

Memory use before, and after, deallocating is shown.  The Min Free column shows the lowest level of struct space available during the progam run,
for tuning purposes.

