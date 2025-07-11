# forth-memory-management
Define an array of items, then allocate and deallocate items from the array.  (multiple instances, of different item size and number, can be made)

Pair a special-purpose stack with an array of items.

The special-purpose stack is initialized with the address of each array item.

Allocation and deallocation is fairly fast because it involves only popping or pushing the stack.

The number (capacity), and size, of the items is configurable at instance creation.

Within the limit of the maximum number (capacity) of items allocated at the same time (which you set), an infinite number of allocations and deallocations are possible.

The files can be brought into gforth with the command: include example.fs

No checking is done to avoid an invalid address being deallocated, 
or a valid address being deallocated more than once.

On deallocation, the first cell of an item is zeroed out, to make use problems apparent.

Allocations, and deallocations, causes the increasing disorder of the
addresses on the stack, which has no effect on the utility, or speed, of the stack-array.

That a stack is used to provide memory management to a stack-based language is sublime.

The examples use linked lists of numbers, Linear Algebra (with lots of calculations and intermediate results) would be possible.

Lists could be of other memory chunks, like objects. They may, or may not, be in a stack-array.

The list-link data address is one cell, it could contain one-cell data directly.

Deallocate each list as soon as it is no longer needed.

The stack-array can be used for memory chunks that are not dependent on lists for processing,
that have an allocation/deallocation life cycle.

Diagnosis of a memory leak can begin with the stack that becomes exhausted.

The first word of every stack item can be set to a large prime number, to indicate the type of memory.  The setting of the first word of a deallocated array item to zero would need to be changed to the second word.  The prime number can be added to the stack-array structure, by shifting the beginning of the array, or stack, up by one word.
