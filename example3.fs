\ Soew list-of-lists building, deallocating.

\ Constants.
 4  constant num-bits
15 constant all-bits
 8  constant ms-bit

include tools2.fs
include mm_array.fs
include link.fs
include list.fs
include region.fs
include regionlist.fs
include state.fs
cs

: memory-use ( -- )
    cr ." Memory use:"
    cr 4 spaces ." Region mma:       " region-mma .mma-usage
    cr 4 spaces ." List mma:         " list-mma .mma-usage
    cr 4 spaces ." Link mma:         " link-mma .mma-usage
    cr 4 spaces ." dstack: " .s
;

\ Init array-stacks.
101 link-mma-init
102 list-mma-init
103 region-mma-init

list-new            \ Root list.

\ Make another list, populate it, store it.
list-new            \ root list1
3 5 region-new over list-push
5 6 region-new over list-push
over list-push      \ root

\ Make another list, populate it, store it.
list-new            \ root list2
1 2 region-new over list-push
2 6 region-new over list-push
over list-push      \ root

\ Finish.
cr
memory-use
cr

\ Deallocate remaining struct instances.
cr ." Deallocating ..."

dup                             \ root root
' region-list-deallocate        \ root root xt
swap list-apply                 \ root
list-deallocate                 \ 

cr memory-use cr

\ Free heap memory before exiting.
cr ." Freeing heap memory"
list-mma mma-free
link-mma mma-free
region-mma mma-free
cr
