
\ Constants.
 #4 constant num-bits
#15 constant all-bits
 #8 constant ms-bit

include globals.fs
include xtindirect.fs
include tools.fs
include tools2.fs
include mm_array.fs
include struct.fs
include link.fs
include list.fs
include structlist.fs
include region.fs
include structinfo.fs
include structinfolist.fs
include stackprint2.fs
cs

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#010 structinfo-mma-init
#002 region-mma-init

\ Init structinfo list.
list-new to structinfo-list-store
' from-string-false ' link-deallocate ' .link s" Link" link-mma link-struct-id structinfo-new structinfo-list-store structinfo-list-push
' from-string-false ' structinfo-list-deallocate-struct-list ' structinfo-list-print-struct-list s" List" list-mma list-struct-id structinfo-new structinfo-list-store structinfo-list-push-end
' from-string-false ' structinfo-deallocate ' .structinfo s" StructInfo" structinfo-mma structinfo-struct-id structinfo-new structinfo-list-store structinfo-list-push-end
' region-from-string ' region-deallocate ' .region s" Region" region-mma region-struct-id structinfo-new structinfo-list-store structinfo-list-push

cr #5 #5 region-new  cr ." Dropped region: " hex. cr
cr #4 #5 region-new  cr ." Dropped region: " hex. cr
#3 #5 region-new 

