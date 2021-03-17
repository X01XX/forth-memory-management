# forth-memory-management
Define an array of items, then allocate and deallocate items from the array.

Pair a special-purpose stack with an array of items.

The special-purpose stack is initialized with the address of each array item.

Allocation and deallocation is fairly fast because it involves only popping or pushing the stack.

The number (capacity), and size, of the items is configurable at instance creation.

Within the limit of the maximum number (capacity) of items allocated at the same time, an infinite number of allocations and deallocations are possible.

The files can be brought into gforth with the command: include example.fs

No checking is done to avoid an invalid address being deallocated, 
or a valid address being deallocated more than once.

On deallocation, the first cell of an item is zeroed out, to make use problems apparent.

The entropy of various allocations, and deallocations, appears in the increasing disorder of the
addresses on the stack, which has no effect on the utility of the stack-array.

That a stack is used to provide memory management to a stack-based language is sublime.

The examples use linked lists of numbers, Linear Algebra would be possible.

Lists could be of other memory chunks, like objects. They may, or may not, be in a stack-array.

Deallocate each list as soon as it is no longer needed.

The stack-array can be used for memory chunks that are not dependent on lists for processing,
that have an allocation/deallocation life cycle.
