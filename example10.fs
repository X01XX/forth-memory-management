
\ Constants.
 #4 constant num-bits
#15 constant all-bits
 #8 constant ms-bit

0 value structinfo-list-store

include tools.fs
include tools2.fs
include struct.fs
include mm_array.fs
include link.fs
include list.fs
include structlist.fs
include structinfo.fs
include structinfolist.fs
include stackprint2.fs
include float32.fs
include float32list.fs
cs

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#010 structinfo-mma-init
#100 float32-mma-init

\ Init structinfo list.
list-new to structinfo-list-store
' link-deallocate ' .link s" Link" link-mma link-id structinfo-new structinfo-list-store structinfo-list-push
' structinfo-list-deallocate-struct-list ' structinfo-list-print-struct-list s" List" list-mma list-id structinfo-new structinfo-list-store structinfo-list-push-end
' structinfo-deallocate ' .structinfo s" StructInfo" structinfo-mma structinfo-id structinfo-new structinfo-list-store structinfo-list-push-end
' float32-deallocate ' f. s" Float32" float32-mma float32-id structinfo-new structinfo-list-store structinfo-list-push

\ Init a list.
list-new                                \ f32-lst

\ Add values.
1.2e float32-new over float32-list-push \ f32-lst
2.2e float32-new over float32-list-push \ f32-lst
3.2e float32-new over float32-list-push \ f32-lst

cr cr ." List: " dup .float32-list cr

\ Add 3.2e to each list element.
3.2e float32-new                        \ f32-lst f32-add

\ Get the execute token for a function to apply.
\ It can be anything that takes two float32 instances and returns a result float32 instance.
' float32-add                           \ f32-lst f32-add xt

\ Get item to use for do-op, and list.
over                                    \ f32-lst f32-add xt f32-add
#3 pick                                 \ f32-lst f32-add xt f32-add f32-lst

\ Do the operation.
float32-do-op                           \ f32-lst f32-add f32-lst2

cr cr ." List after adding 3.2e: " dup .float32-list cr

float32-list-deallocate
float32-deallocate
float32-list-deallocate

\ Finish.
cr structinfo-list-store structinfo-list-print-memory-use cr

\ Deallocate remaining struct instances.
cr ." Deallocating ..."

cr structinfo-list-store structinfo-list-print-memory-use cr

structinfo-list-store structinfo-list-project-deallocated

\ Free heap memory before exiting.
." Freeing heap memory"
structinfo-list-store structinfo-list-free-heap
cr
