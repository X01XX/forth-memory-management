The current version is on https://github.com/X01XX/forth-memory-management

Run > gforth

then: include example.fs

Strategy: Build one thing upon another.

1) A stand-alone stack (SAS), with an array of cells for stack items,
   and a cell for storing the stack Pointer.

2) A Memory Management array (MMA), with an array of potential struct instances,
   and a SAS to hold addresses of the array items.

3) A link struct, holding data and next-link cells, using a MMA.

4) A list struct, holding a link cell, using a MMA, and links.
   Enables an empty, no-link, list.

5) Add a header cell to structs, holding a 16-bit ID number, and a 16-bit use-count number.
   The list struct adds a 16-bit length field.

6) Enabling all sorts of lists, of various struct instances.

