
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

