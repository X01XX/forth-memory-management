Current version on https://github.com/X01XX/forth-memory-management

Numbers are items in an mma-store, but its possible they could occupy the data cell in a link directly.

Run > gforth

then: include example.fs

Build one thing upon another:

1) A stand-alone stack, SAS, with an array and a cell for storing the stack Pointer.

2) A Memory Management array (MMA), using an array and a SAS.

3) Link struct, holding data and next-link cells, using a MMA.

4) List struct, holding a link cell, using a MMA, and links.
   Enables an empty, no-link, list.

5) All sorts of lists, of various struct instances.

