\ Show list-of-lists, printing, deallocating, without structinfo functions.

\ Constants.
 #4  constant num-bits
#15 constant all-bits
 #8  constant ms-bit

include tools.fs
include tools2.fs
include struct.fs
include mm_array.fs
include link.fs
include list.fs
include structlist.fs
include region.fs
include regionlist.fs
include state.fs
include stackprint.fs
cs

: memory-use ( -- )
    cr ." Memory use:"
    cr #4 spaces ." List mma:         " list-mma .mma-usage
    cr #4 spaces ." Link mma:         " link-mma .mma-usage
    cr #4 spaces ." Region mma:       " region-mma .mma-usage
    cr #4 spaces ." dstack: "
    base @ >r decimal .s r> base !
;

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#102 region-mma-init

list-new                    \ root-lst.

\ Add a few numbers.
#1 over list-push
#4 over list-push

\ Make another list, populate it, store it.
list-new                    \ root-lst list1
#3 over list-push
#5 over list-push
over list-push-struct       \ root-lst

\ Make another list, populate it, store it.
list-new                    \ root-lst list2
#2 over list-push
#6 over list-push
over list-push-end-struct   \ root-lst

\ Copy root list.
dup list-copy               \ root-lst copy-lst

\ Add a number to the beginning of the copy list, to distinguish it.
list-new
#2 over list-push
#7 over list-push
over list-push
1 over list-push            \ root-lst copy-lst

cr cr ." List of lists root: " ' . #2 pick .list cr
cr cr ." List of lists copy push extra list: " ' . over .list cr

' = #3 #2 pick list-find    \ root-lst copy-lst, item t | f
cr ." expect 3 -1, found: " swap . . cr

' = #9 #2 pick list-find    \ root-lst copy-lst, item t | f
cr ." expect 0, found: " . cr

' = #4 #2 pick list-find    \ root-lst copy-lst, item t | f
cr ." expect 4 -1, found: " swap . . cr

' = #2 #2 pick list-find-all \ root-lst copy-lst lst
cr ." expect (2 2), found: " ' . over .list cr
list-deallocate

' = #4 #2 pick list-find-all \ root-lst copy-lst lst
cr ." expect (4), found: " ' . over .list cr
list-deallocate

' = #7 #2 pick list-member  \ root-list copy-list bool
cr ." expect t, found: " . cr

' = #4 #2 pick list-member  \ root-list copy-list bool
cr ." expect t, found: " . cr

' = #9 #2 pick list-member  \ root-list copy-list bool
cr ." expect f, found: " . cr

cr ." Numbers in order: " ' . over list-apply cr

' 2drop-true 0 #2 pick list-find-all
cr ." flattened list: " ' . over .list cr

\ Make structure list.
list-new                    \ root-lst copy-lst reg-lst
#5 #4 region-new over list-push-struct
#1 #4 region-new over list-push-struct
list-new
#1 #3 region-new over list-push-struct
#7 #3 region-new over list-push-struct
over list-push-struct

dup list-copy-struct
#0 #0 region-new over list-push-struct
cr cr ." Region list: " over .region-list
cr cr ." Region list copy + 0000: " dup .region-list cr

' 2drop-true 0 #2 pick list-find-all-struct
cr ." flattened list: " dup .region-list cr

\ Finish.
cr
memory-use
cr

\ Deallocate remaining struct instances.
cr ." Deallocating ..."
                                \ root copy flat reg-lst reg-copy reg-f
region-list-deallocate          \ root copy flat reg-lst reg-copy

' region-deallocate over list-apply-recursive
list-deallocate-recursive

' region-deallocate over list-apply-recursive
list-deallocate-recursive       \ root copy

list-deallocate                 \ root

list-deallocate-recursive       \

list-deallocate-recursive       \

cr memory-use cr

assert-list-mma-none-in-use
assert-link-mma-none-in-use
assert-region-mma-none-in-use
assert-forth-stack-empty

\ Free heap memory before exiting.
cr ." Freeing heap memory"
list-mma mma-free
link-mma mma-free
region-mma mma-free
cr

