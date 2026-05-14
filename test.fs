
\ Constants.
#4 constant num-bits
#15 constant all-bits
#8 constant ms-bit

include xtindirect.fs
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
include list2.fs
include region.fs
cs

\ Init array-stacks.
#101 link-mma-init
#102 list-mma-init
#010 structinfo-mma-init
#020 token-mma-init
#020 region-mma-init



\ Finish.
\ Init structinfo list.
list-new to structinfo-list-store
' from-string-false ' link-deallocate ' .link s" Link" link-mma link-id structinfo-new structinfo-list-store structinfo-list-push
' from-string-false ' structinfo-list-deallocate-struct-list ' structinfo-list-print-struct-list s" List" list-mma list-id structinfo-new structinfo-list-store structinfo-list-push-end
' from-string-false ' structinfo-deallocate ' .structinfo s" StructInfo" structinfo-mma structinfo-id structinfo-new structinfo-list-store structinfo-list-push-end

\ The list, link, and StructInfo structs allow for the creation of the structinfo-list-store,

' from-string-false ' token-deallocate ' .token s" Token" token-mma token-id structinfo-new structinfo-list-store structinfo-list-push-end
' region-from-string ' region-deallocate ' .region s" Region" region-mma region-id structinfo-new structinfo-list-store structinfo-list-push-end

