\ Implement a region struct and functions.

19317 constant region-id
   3 constant region-struct-number-cells

\ Struct fields
0 constant region-header
region-header  cell+ constant region-state-0
region-state-0 cell+ constant region-state-1

0 value region-mma \ Storage for region mma instance.

\ Init region mma, return the addr of allocated memory.
: region-mma-init ( num-items -- ) \ sets region-mma.
    dup 1 <
    if
        ." region-mma-init: Invalid number of items."
        abort
    then

    cr ." Initializing Region store."
    region-struct-number-cells swap mma-new to region-mma
;

\ Check instance type.
: is-allocated-region ( addr -- flag )
    \ Insure the given addr cannot be an invalid addr.
    dup region-mma mma-within-array 0=
    if
        drop false exit
    then

    struct-get-id   \ Here the fetch could abort on an invalid address, like a random number.
    region-id =     
;

: is-not-allocated-region ( addr -- flag )
    is-allocated-region 0=
;

\ Check arg0 for region, unconventional, leaves stack unchanged. 
: assert-arg0-is-region ( arg0 -- arg0 )
    dup is-allocated-region 0=
    if
        cr ." arg0 is not an allocated region"
        abort
    then
;

\ Check arg1 for region, unconventional, leaves stack unchanged. 
: assert-arg1-is-region ( arg1 arg0 -- arg1 arg0 )
    over is-allocated-region 0=
    if
        cr ." arg1 is not an allocated region"
        abort
    then
;


\ Start accessors.

\ Return the first field from a region instance.
: region-get-state-0 ( addr -- u)
    \ Check arg.
    assert-arg0-is-region

    region-state-0 +    \ Add offset.
    @                   \ Fetch the field.
;
 
\ Return the second field from a region instance.
: region-get-state-1 ( addr -- u)
    \ Check arg.
    assert-arg0-is-region

    \ Get second state.
    region-state-1 +    \ Add offset.
    @                   \ Fetch the field.
;
 
\ Set the first field from a region instance, use only in this file.
: _region-set-state-0 ( u1 addr -- )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-value

    region-state-0 +    \ Add offset.
    !                   \ Set first field.
;
 
\ Set the second field from a region instance, use only in this file.
: _region-set-state-1 ( u1 addr -- )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-value

    region-state-1 +    \ Add offset.
    !                   \ Set second field.
;

\ End accessors.

\ Push a region to a region-list.
: region-list-push ( reg1 list0 -- )
    \ Check args.
    assert-arg0-is-list
    assert-arg1-is-region

    over struct-inc-use-count
    list-push
;

\ Create a region from two numbers on the stack.
\ The numbers may be the same.
\ If you want to keep the region on the stack, or in a value, or variable,
\ run dup struct-inc-use-count, then deallocate it from there when done using it.
\ If you want to push the region onto a list, region-list-push will increment the use count.
: region-new ( u1 u0 -- addr)
    \ Check args.
    assert-arg0-is-value
    assert-arg1-is-value

    \ Allocate space.
    region-mma mma-allocate     \ u1 u2 addr

    \ Store id.
    region-id over              \ u1 u2 addr id addr
    struct-set-id               \ u1 u2 addr
    
    \ Init use count.
    0 over struct-set-use-count

    \ Prepare to store states.
    -rot            \ addr u1 u2
    2 pick          \ addr u1 u2 addr
    swap over       \ addr u1 addr u2 addr

    \ Store states
    _region-set-state-1     \ addr u1 addr
    _region-set-state-0     \ addr
;

\ Print a region.
: .region ( reg0 -- )
    \ Check arg.
    assert-arg0-is-region

    \ Setup for bit-position loop.
    dup  region-get-state-1
    swap region-get-state-0
    ms-bit              \ st2 st1 ms-bit
    
    \ Process each bit.
    begin
      dup
    while
      \ Apply msb to state 1.
      over      \ Get state 1.
      over      \ Get msb and isolate state 1 bit.
      and       \ Isolate state 1 bit corresponding to the msb.

      \ Apply msb to state 2.
      over      \ Get msb.
      4 pick    \ Get state2 and isolate 1 bit.
      and       \ Isolate state 2 bit corresponding to the msb.

      if
          if
            ." 1"
          else
            ." x"
          then
      else
          if
            ." X"
          else
            ." 0"
          then
      then

      1 rshift
    repeat
    2drop drop  \ Drop state1 state2 msb.
;

\ Return the highest state in a region.
: region-high-state ( reg0 -- n )
    \ Check arg.
    assert-arg0-is-region

    dup  region-get-state-0    \ reg0 state1.
    swap region-get-state-1    \ state1 state2.
    or                         \ High state.
;

\ Return the lowest state in a region.
: region-low-state ( reg0 -- n )
    \ Check arg.
    assert-arg0-is-region

    dup  region-get-state-0    \ addr state1.
    swap region-get-state-1    \ state1 state2.
    and                        \ Low state.
;

\ Deallocate a region.
: region-deallocate ( reg0 -- )
    \ Check arg.
    assert-arg0-is-region

    dup struct-get-use-count      \ reg0 count

    2 <
    if 
        \ Clear fields.
        0 over _region-set-state-0
        0 over _region-set-state-1

        \ Deallocate instance.
        region-mma mma-deallocate
    else
        struct-dec-use-count
    then
;

\ Return the two states that make a region.
: region-get-states ( reg0 -- state1 state2 )
    \ Check arg.
    assert-arg0-is-region

    \ Calc result.
    dup region-get-state-0
    swap
    region-get-state-1
;

\ Return a regions edge mask.
: region-edge-mask ( reg0 -- mask )
    \ Check arg.
    assert-arg0-is-region

    \ Calc result.
    region-get-states
    !nxor
;

\ Return true if two regions intersect.
\ And diff-bits in a state from each region.
\     same bits mask from reg1
\     same bits mask from reg2
\ Return 0=
: region-intersects ( reg1 reg0 -- flag )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-region

    \ Get different bits, a superset of such bits.
    over region-get-state-0
    over region-get-state-0
    xor
    -rot     \ dif reg1 reg2

    \ Get reg2 edge bits.
    region-edge-mask

    \ Get reg1 edge bits.
    swap region-edge-mask

    \ And the three
    and and

    \ Return result
    0=
;

\ Return the region high state and low state.
: region-high-low ( reg0 -- high low )
    \ Check arg.
    assert-arg0-is-region

    \ Calc result.
    dup region-high-state
    swap region-low-state
;

\ Return the intersection of two regions, or false if they do not intersect.
\ Since this must check for intersection first, there may be no need to check 
\ for intersection before calling this.
: region-intersection ( reg1 reg0 -- reg true | false )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-region

    \ Check that the two regions intersect.
    2dup region-intersects
    if
        \ reg1 reg0
        \ Get high and low state of reg0
        region-high-low     \ reg1 reg0high reg0low

        \ Get high and low state of reg1
        rot region-high-low \ reg0high reg0low reg1high reg1low
  
        \ Group high/low states.
        rot                 \ reg0high reg1ghigh reg1low reg0low
  
        \ Calc result
        or -rot and
        region-new
        true
    else
        2drop
        false
    then
;

\ Return the union of two regions.
: region-union ( reg1 reg0 -- reg3 )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-region

    \ reg1 reg0
    \ Get high and low state of reg0
    region-high-low     \ reg1 reg0high reg0low

    \ Get high and low state of reg1
    rot region-high-low \ reg0high reg0low reg1high reg1low

    \ Group high/low states.
    rot                 \ reg0high reg1high reg1low reg0low

    \ Calc result
    and -rot or
    region-new
;

\ Return a regions X mask.
: region-x-mask ( reg0 -- mask )
    \ Check arg.
    assert-arg0-is-region

    \ Calc result.
    region-get-states
    xor
;

\ Return a regions 1-1 mask.
: region-1-mask ( reg0 -- mask )
    \ Check arg.
    assert-arg0-is-region

    \ Calc result.
    region-get-states
    and
;

\ Return a regions 0-0 mask.
: region-0-mask ( reg0 -- mask )
    \ Check arg.
    assert-arg0-is-region

    \ Calc result.
    region-get-states
    !nor
;

\ Return a new region with some X positions set to zero.
\ Change 1-0 or 0-1 to 0-0.
\ Mask will usually have a single bit, called from region-subtract.
: region-x-to-0 ( to-0-mask reg0 -- reg )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-value

    region-get-states           \ to-0-mask state1 state2
    rot !not                \ state1 state2 keep-mask
    swap over               \ state1 keep state2 keep
    and                     \ state1 keep state2'
    -rot                    \ state2' state1 keep
    and                     \ state2' state1'
    region-new              \ reg
;

\ Return a new region with some X positions set to one.
\ Change 1-0 or 0-1 to 1-1.
\ Mask will usually have a single bit, called from region-subtract.
: region-x-to-1 ( to-1-mask reg0 -- reg )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-value

    region-get-states           \ to-1 state1 state2
    rot                     \ state1 state2 to-1 
    swap over               \ state1 to-1 state2 to-1
    or                      \ state1 to-1 state2'
    -rot                    \ state2' state1 to-1
    or                      \ state2' state1'
    region-new              \ reg
;

\ Return true if two regions are equal.
: region-eq ( reg1 reg0 -- flag )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-region

    over region-high-state over region-high-state <>
    if
        2drop
        false
        exit
    then

    region-low-state swap region-low-state <>
    if
        false
    else
        true
    then
;

\ Return true if a region (TOS) is a superset of the next region on stack.
: region-superset-of ( reg1 reg0 -- flag )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-region

    2dup region-intersects          \ reg1 reg0 flag
    if
        \ Regions intersect.
        over region-intersection    \ reg1 reg-int flag
        if
            dup struct-inc-use-count
                                    \ reg1 reg-int
            swap over region-eq     \ reg-int flag
            swap region-deallocate  \ flag
            \ dup ."  = " .
            \ exit
        else
            ." region-superset-of: reg0 and reg1 should intersect"
            abort
        then
    else
        \ Regions do not intersect, return false.
        2drop
        false
    then
;

\ Return true if a region (TOS) is a subset of the next region on the stack.
: region-subset-of ( reg1 reg0 -- flag )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-region

    2dup region-intersects          \ reg1 reg0 flag
    if
        \ Regions intersect.
        swap                        \ reg0 reg1
        over region-intersection    \ reg0 reg-int flag
        if
            dup struct-inc-use-count
                                    \ reg0 reg-int
            swap over region-eq     \ reg-int flag
            swap region-deallocate  \ flag
            \ dup ."  = " .
            \ exit
        else
            ." region-subset-of: reg0 and reg1 should intersect"
            abort
        then
    else
        \ Regions do not intersect, return false.
        2drop
        false
    then
;

\ Return true if a region (TOS) is a superset of the next state on stack.
: region-superset-of-state ( sta1 reg0 -- flag )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-value

    region-get-states           \ sta1 rsta1 rsta2
    rot                         \ rsta1 rsta2 sta1
    swap over                   \ rsta1 sta1 rsta2 sta1
    xor                         \ rsta1 sta1 diff2
    -rot                        \ diff2 rsta1 sta1
    xor                         \ diff2 diff1
    and                         \ both-diff
    0=                          \ flag
;

\ Return a region-list from a region (TOS) minus a second region.
: region-subtract ( reg1 reg0 -- region-list )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-region

    \ Check if any subtraction is needed.
    2dup region-intersects 0=       \ reg1 reg0 flag
    if
        list-new swap over          \ reg1 list reg0 list
        region-list-push            \ reg1 list
        nip                         \ list
        exit
    then

    \ Check if the result is nothing.
    2dup swap                       \ reg1 reg0 reg0 reg1
    region-superset-of              \ reg1 reg0 flag
    if
        2drop
        list-new
        exit
    then

    \ Init return list
    list-new                        \ reg1 reg0 list

    \ Change x over 1 positions to 0 over 1, one position at a time.
                                    \ reg1 reg0 list
    over region-x-mask              \ reg1 reg0 list | xmask
    3 pick region-1-mask            \ reg1 reg0 list | xmask 1mask
    and                             \ reg1 reg0 list | x1mask

    begin
        dup
    while
        isolate-a-bit               \ reg1 reg0 list | x1mask' one-bit
        3 pick                      \ reg1 reg0 list | x1mask' one-bit reg0
        region-x-to-0               \ reg1 reg0 list | x1mask' reg0'
        2 pick region-list-push     \ reg1 reg0 list | x1mask'
    repeat
    drop                            \ reg1 reg0 list

    \ Change x over 0 positions to 1 over 0, one position at a time.
                                    \ reg1 reg0 list
    over region-x-mask              \ reg1 reg0 list | xmask
    3 pick region-0-mask            \ reg1 reg0 list | xmask 0mask
    and                             \ reg1 reg0 list | x0mask
    begin
        dup
    while
        isolate-a-bit               \ reg1 reg0 list | x0mask' one-bit
        3 pick                      \ reg1 reg0 list | x0mask' one-bit reg0
        region-x-to-1               \ reg1 reg0 list | x0mask' reg0'
        2 pick region-list-push     \ reg1 reg0 list | x0mask'
    repeat
    drop                            \ reg1 reg0 list

    nip nip                         \ list
;

\ Return a region-list from a region (TOS) minus a state.
: region-subtract-state ( sta1 reg0 -- region-list )
    \ Check args.
    assert-arg0-is-region
    assert-arg1-is-value

    \ Check if any subtraction is needed.
    2dup region-superset-of-state 0=    \ sta1 reg0 flag
    if
        list-new swap over          \ sta1 list reg0 list
        region-list-push            \ sta1 list
        nip                         \ list
        exit
    then

    \ Init return list
    list-new                        \ sta1 reg0 list

    \ Change x over 1 positions to 0 over 1, one position at a time.
                                    \ sta1 reg0 list
    over region-x-mask              \ sta1 reg0 list | xmask

    \ Check if the result is nothing.
    dup 0=                          \ sta1 reg0 list | xmask
    if
        drop
        swap drop
        swap drop
        exit
    then

    3 pick                          \ sta1 reg0 list | xmask 1mask
    and                             \ sta1 reg0 list | x1mask

    begin
        dup
    while
        isolate-a-bit               \ sta1 reg0 list | x1mask' one-bit
        3 pick                      \ sta1 reg0 list | x1mask' one-bit reg0
        region-x-to-0               \ sta1 reg0 list | x1mask' reg0'
        2 pick                      \ sta1 reg0 list | x1mask' reg0 list
        region-list-push            \ sta1 reg0 list | x1mask'
    repeat
    drop                            \ sta1 reg0 list

    \ Change x over 0 positions to 1 over 0, one position at a time.
                                    \ sta1 reg0 list
    over region-x-mask              \ sta1 reg0 list | xmask
    3 pick !not                     \ sta1 reg0 list | xmask 0mask
    and                             \ sta1 reg0 list | x0mask
    begin
        dup
    while
        isolate-a-bit               \ sta1 reg0 list | x0mask' one-bit
        3 pick                      \ sta1 reg0 list | x0mask' one-bit reg0
        region-x-to-1               \ sta1 reg0 list | x0mask' reg0'
        2 pick region-list-push     \ sta1 reg0 list | x0mask'
    repeat
    drop                            \ sta1 reg0 list

    nip nip                         \ list
;
