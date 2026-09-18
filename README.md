# forth-memory-management
Pair an array of potential struct instances, with a special-purpose stack, giving an array-stack.

Multiple array-stacks can be created, with the number of items, and the size of each item, varying as needed.
Different array-stacks could be accessed in parallel, no mutex.

The special-purpose stack does not use a dedicated register, it stores position information in the stack struct header.
This requires additional cycles for a load and a store, but allows any number of stacks.

The stack of an array-stack is initialized with the address of each array item.  The maximum required size of the stack is known.

Allocation and deallocation is fairly fast because it involves simply popping, or pushing, the stack.

Within the limit of the maximum number of array items allocated at the same time,
an infinite number of allocations, and deallocations, are possible.

Allocations, and deallocations, cause increasing disorder of the addresses on the stack,
which has no effect on the utility, or speed, of the array-stack.

From low to high memory usage, there is no effect on performance, no memory fragmentation.

That a stack is used to provide memory management to a stack-based language is sublime.
Forth runs on an OS that uses stacks. Its stacks, all the way down.

Functions that manipulate a struct instance can act as wrappers to array-stack, and list, functions.

At program end, each address in the array should have been returned to the stack, although not in the same order.
At that point, a function can be run to detect memory leaks. The type of struct (the specific array-stack), the
specific instance addresses, and a hex dump of each leaked struct instance will be printed.

There is a process for finding where any lost struct instance is allocated, then follow your code to where it
should be deallocated. See memory-leak-fixing.odt.

Changes "There are no memory leaks currently causing a problem" to "There are no memory leaks".

At first, making a change and seeing a memory leak causes frustration. But they are not that hard to find and fix,
since the change is still in your mind. It later becomes a welcome warning, fix this before you make more changes.

Knowing the addresses in the array allows detection of the deallocation of an invalid address.
The address is between the start and end of the array, the address minus the start of the array, mod the struct instance size equals zero.

For greater speed, some checks can be made active for debug mode, inactive for production.

The first word of every struct instance, allocated from the same array-stack, can be set to a unique number,
to indicate the type of struct.

On deallocation, the first cell of an item is zeroed out, to make use problems apparent.
Like using a deallocated instance, or double-deallocation.

These ideas should work in Assembler Language (see the risc-v-memory-management-vf2 project) and C.

Lists are built of List structs, and Link structs, that have an ID and use-count in their headers.

The examples can be run with the commands:
<pre>
  gforth example.fs   \ Shows two lists of numbers, with set functions union, intersection and subtraction.
                      \ Shows applying multiplication, and addition, of all list elements, given a number.
                      \ The numbers are not structs, just numbers in the Link data field.
                      \ Shows two lists of names, with set functions union, intersection and subtraction. 
                      \ The Name is a struct.

  gforth example2.fs  \ Shows my favorite equation, Understanding = ~A + ~B.
                      \ See more detailed notes under the UES-Forth project.
                      \ UES-Forth is a 30K+ line application that uses this code to make 25 different structs and 20 struct lists.
                      \ At end it deallocates around two thousand struct instances, checks for memory leaks, and anything
                      \ left on the Forth stack.
                      \ Having made around 14 Million struct allocation/deallocation operations.
                      \ A lot of activity, in less than 1 MB of data.

  gforth example4.fs  \ Shows a struct-aware print of items on the Forth stack, for debugging.

  gforth example6.fs  \ Test list functions that can work with lists with sub-lists.

  gforth example7.fs  \ Shows a link, list and region, being lost, and detected later.

  gforth example8.fs  \ Shows a stack empty event, with one lost struct instance reported.
  
  gforth example10.fs \ Shows a list of floating point numbers, and an operation on the list, producing a second list.

  gforth example11.fs \ Shows string parsing, print, and deallocate, of a list of mixed float, integer, sub-list,
                      \ region, and an unidentified token.
                      \ Only fails with unbalanced parentheses, or unidentified token GT token struct string limit,
                      \ currently 80.
                      \ Outer parentheses are optional.
</pre>

Memory use before, and after, deallocating is shown.  The Min Free column shows the lowest level of struct instances available during the program run,
for tuning purposes.

Total number of allocations, per struct kind, are shown in the memory print.  At end, the deallocations equal the allocations.

Stacks can be created, and used, without an array.  This is used in list2.fs, list-from-token-list.

For a struct that uses one cell, the overhead of the stack is about 50%.  For a 10-cell struct, its about 10%.
