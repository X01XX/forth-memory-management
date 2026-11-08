
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
include token.fs
cs

\ Init array-stacks.
#020 token-mma-init

\ Show detection of invalid address.
s" xxx" token-new           \ tkn
dup token-deallocate        \ tkn-u

\ Show double deallocation of struct error.
token-mma mma-deallocate-struct

