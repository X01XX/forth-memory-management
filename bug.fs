FYI

Submitting a bug report to GNU seems to be non-trivial.

In the following, the abort" command seems to mess up.

: isolate-a-bit ( u1 -- u2 u3 )
    dup 0=
    if
       abort" isolate-a-bit: argument is zero"                                                                                                                                           
    then

    dup 1- over and 

    swap over xor 
;

The command:  0 isolate-a-bit

Seems to skip the abort, consume two items from the stack, then bomb on dup in the
"dup  1-  over  and"  line.
