# forth-memory-management
Define an array of items, then allocate and deallocate items from the array.

Pair a special-purpose stack with an array of items.

The number, and size, of the items is configurable at instance creation.

For a different number and/or size of items, create another instance.

The special-purpose stack is initialized with the address of each array item.

Allocation and deallocation is fairly fast because it involves only pushing or popping the stack.

The entropy of various allocations and deallocations appears in the order of the
addresses on the stack, which no one cares about.

The files can be brought into gforth with the command: include mm_array.fs

No checking is done to avoid an invalid address being deallocated, 
or a valid address being deallocated twice.
