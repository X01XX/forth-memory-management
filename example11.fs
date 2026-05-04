
\ Constants.
#4 constant num-bits
#15 constant all-bits
#8 constant ms-bit

include tools.fs
include tools2.fs
include mm_array.fs
include struct.fs
include link.fs
include list.fs
include structlist.fs
include structinfo.fs
include structinfolist.fs
include stackprint2.fs
include token.fs
include tokenlist.fs
include integerlist.fs
cs

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#010 structinfo-mma-init
#020 token-mma-init

: token-test ( c-addr u -- )
    token-list-from-string             \ tkn-lst t | f

    if
        dup integer-list-from-token-list                \ tkn-lst, int-lst t | f
        if
            space ." integer-list: " dup .integer-list                          \ tkn-lst int-lst

            \ Clean up.
            integer-list-deallocate
            token-list-deallocate
        else
            space ." integer-list-from-token-list failed"

            \ Clean up.
            token-list-deallocate
        then
    else
        space ." token list error, unbalanced parens?"
    then
;

\ Finish.
\ Init structinfo list.
list-new to structinfo-list-store
' link-deallocate ' .link s" Link" link-mma link-id structinfo-new structinfo-list-store structinfo-list-push
' structinfo-list-deallocate-struct-list ' structinfo-list-print-struct-list s" List" list-mma list-id structinfo-new structinfo-list-store structinfo-list-push-end
' structinfo-deallocate ' .structinfo s" StructInfo" structinfo-mma structinfo-id structinfo-new structinfo-list-store structinfo-list-push-end

\ The list, link, and StructInfo structs allow for the creation of the structinfo-list-store,

' token-deallocate ' .token s" Token" token-mma token-id structinfo-new structinfo-list-store structinfo-list-push-end

cr cr
s" (1 2 3 ( 4 5 6))" 2dup type token-test cr cr
s" (1 2 3 ( 4 5 6)x" 2dup type token-test cr cr
s" (1 2 3 ( 4 5 6)))" 2dup type token-test cr cr
s" )1 2 3 ( 4 5 6))" 2dup type token-test cr cr
s" (1 2 3 ( a 5 6))" 2dup type token-test cr cr
s" 1 2 3" 2dup type token-test cr cr
s" 1 (3 4) 2 3" 2dup type token-test cr cr

cr ." Check memory ..." cr

structinfo-list-store structinfo-list-print-memory-use

structinfo-list-store structinfo-list-project-deallocated

\ Free heap memory before exiting.
." Freeing heap memory"
structinfo-list-store structinfo-list-free-heap
