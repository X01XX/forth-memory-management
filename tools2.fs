
\ Return true if a number is an invalid value.
: is-not-value ( u -- flag )
  dup all-bits and 
  <>  
;  

\ Return the bitwise "NOT" of an unsigned number,
\ while remaining within the bounds of allowable bits.
: !not ( u1 -- u2 )
    all-bits
    xor
;

\ Return the bitwise "NXOR" of two unsigned numbers.
\ while remaining within the bounds of allowable bits.
: !nxor ( u1 u2 -- u3 )
    xor !not
;

\ Return the bitwise "NOR" of two unsigned numbers.
\ while remaining within the bounds of allowable bits.
: !nor ( u1 u2 -- u3 )                                                                              
    or !not
;

\ Isolate LSB from a non-zero number.
\ Return changed number and a single-bit number.
: isolate-a-bit ( u1 -- u2 u3 )                                                                     
    depth
    0= if
       ." isolate-a-bin: no argument on stack"
       abort
    then
    dup 0=
    if  
       ." isolate-a-bit: argument is zero"
       abort
    then
    \ Remove lsb.
    dup 1- over and     \ u u-lsb 

    \ Isolate lsb.
    swap over xor       \ u-lsb lsb
;

\ Return true if a number is a valid value.
: is-value ( u -- flag )
    dup all-bits and 
    =   
;
 
\ Check arg0 for value, unconventional, leaves stack unchanged. 
: assert-arg0-is-value ( u -- u )
    dup is-not-value
    if  
        ." arg0 is not a valid value."
        abort
    then
;

\ Check arg1 for value, unconventional, leaves stack unchanged. 
: assert-arg1-is-value ( u ?? -- u ??)
    over is-not-value
    if  
        ." arg1 is not a valid value."
        abort
    then
;

\ Check arg2 for value, unconventional, leaves stack unchanged. 
: assert-arg2-is-value ( u ?? ?? -- u ?? ??)
    #2 pick is-not-value
    if  
        ." arg2 is not a valid value."
        abort
    then
;

