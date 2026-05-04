
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
include floatnum.fs
include floatnumlist.fs
include token.fs
include tokenlist.fs
cs

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#010 structinfo-mma-init
#100 floatnum-mma-init
#020 token-mma-init

\ Init structinfo list.
list-new to structinfo-list-store
' link-deallocate ' .link s" Link" link-mma link-id structinfo-new structinfo-list-store structinfo-list-push
' structinfo-list-deallocate-struct-list ' structinfo-list-print-struct-list s" List" list-mma list-id structinfo-new structinfo-list-store structinfo-list-push-end
' structinfo-deallocate ' .structinfo s" StructInfo" structinfo-mma structinfo-id structinfo-new structinfo-list-store structinfo-list-push-end

\ The list, link, and StructInfo structs allow for the creation of the structinfo-list-store,

' floatnum-deallocate ' .floatnum s" FloatNum" floatnum-mma floatnum-id structinfo-new structinfo-list-store structinfo-list-push-end
' token-deallocate ' .token s" Token" token-mma token-id structinfo-new structinfo-list-store structinfo-list-push-end

\ Init a list.
list-new                                    \ fnum-lst

\ Add values.
1.2e floatnum-new over floatnum-list-push   \ fnum-lst
2.2e floatnum-new over floatnum-list-push   \ fnum-lst
3.2e floatnum-new over floatnum-list-push   \ fnum-lst

cr cr ." List: " dup .floatnum-list cr

\ Add 3.2e to each list element.
3.2e floatnum-new                           \ fnum-lst fnum-add

\ Get the execute token for a function to apply.
\ It can be anything that takes two floatnum instances and returns a result floatnum instance.
' floatnum-add                              \ fnum-lst fnum-add xt

\ Get item to use for do-op, and list.
over                                        \ fnum-lst fnum-add xt fnum-add
#3 pick                                     \ fnum-lst fnum-add xt fnum-add fnum-lst

\ Do the operation.
floatnum-list-do-op                         \ fnum-lst fnum-add fnum-lst2

cr cr ." List after adding 3.2e: " dup .floatnum-list cr

\ Finish.
cr structinfo-list-store structinfo-list-print-memory-use cr

\ Deallocate remaining struct instances.
cr ." Deallocating ..."

floatnum-list-deallocate
floatnum-deallocate
floatnum-list-deallocate

cr structinfo-list-store structinfo-list-print-memory-use cr

structinfo-list-store structinfo-list-project-deallocated

\ Free heap memory before exiting.
." Freeing heap memory"
structinfo-list-store structinfo-list-free-heap
cr
