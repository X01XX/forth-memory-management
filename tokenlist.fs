\ Functions for a list of tokens.

\ Deallocate a token list.
: token-list-deallocate ( token-lst -- )
    dup struct-get-use-count                    \ token-lst uc
    #2 < if
        [ ' token-deallocate ] literal over     \ token-lst xt token-lst
        list-apply                              \ Deallocate token instances in the list.

        list-deallocate                             \ Deallocate list and links.
    else
        struct-dec-use-count
    then
;

\ Check if tos is an empty list, or has a token instance as its first item.
: assert-tos-is-token-list ( tos -- tos )
    assert-tos-is-list
    dup list-is-empty?
    if
    else
        dup list-get-links link-get-data
        assert-tos-is-token
        drop
    then
;

\ Check if nos is an empty list, or has a token instance as its first item.
: assert-nos-is-token-list ( nos tos -- nos tos )
    assert-nos-is-list
    over list-is-empty?
    if
    else
        over list-get-links link-get-data
        assert-tos-is-token
        drop
    then
;

\ Print a token-list
: .token-list ( list0 -- )
    \ Check arg.
    assert-tos-is-token-list

    [ ' .token ] literal swap .list
;

\ Return the depth of a token list,
\ that is how deep the lists go.
: token-list-depth ( tkn-lst - u )
    \ Check arg.
    assert-tos-is-token-list

    \ Init counter.
    0 swap                          \ cnt tkn-lst

    \ Prep for loop.
    list-get-links                  \ cnt link

    begin
        ?dup
    while
        \ Check for left paren.
        s" ("                       \ cnt link c-addr u
        #2 pick link-get-data       \ cnt link c-addr u tkt
        token-eq-string             \ cnt link flag
        if
            \ Inc paren counter.
            swap 1+ swap
        then

        link-get-next
    repeat
                                    \ cnt
;

\ Given a string on the stack, return a token list.
\ Separators are space, comma, left paren, and right paren.
\
\ s" aaa ( bbb ccc) ddd" token-list-from-string drop .token-list (aaa ( bbb ccc ) ddd) ok
\ Tokens returned are: list of: "aaa" "(" "bbb" "ccc" ")" "ddd"
\
\ No parens is Ok.
\ Sub lists, like "( 3 ( 4 3) ( 5 4))" are Ok.
\
\ Returns false if parens are imbalanced.
: token-list-from-string ( c-addr u -- tkn-lst t | f )
    \ Check for null input.
    dup 0= if                       \ c-addr 0
        2drop
        list-new
        exit
    then

    \ Init return list.
    list-new -rot                   \ ret-lst c-addr u

    \ Init token start index.
    0                               \ ret-lst c-addr u str

    \ Setup loop range
    over 0                          \ ret-lst c-addr u str u 0

    \ Scan the string.
    do                              \ ret-lst c-addr u str
        \ Get next char
        #2 pick i + c@              \ ret-lst c-addr u str chr

        \ Evaluate char.
        case
            \ Check for left paren.
             $28 of                 \ ret-lst c-addr u str
                dup i < if
                    \ Add previous token.

                    \ Calc string start.
                    #2 pick         \ ret-lst c-addr u str c-addr
                    over +          \ ret-lst c-addr u str c-addr+

                    \ Calc len.
                    i               \ ret-lst c-addr u str c-addr+ i
                    #2 pick         \ ret-lst c-addr u str c-addr+ i str
                    -               \ ret-lst c-addr u str c-addr+ u

                    \ Make token.
                    token-new       \ ret-lst c-addr u str tkn

                    \ Store token.
                    #4 pick         \ ret-lst c-addr u str tkn ret-lst
                    list-push-end-struct
                then

                \ Add left paren token.
                s" (" token-new     \ ret-lst c-addr u str tkn
                #4 pick             \ ret-lst c-addr u str tkn ret-lst
                list-push-end-struct

                \ Set new start addr.
                drop i 1+
            endof
            \ Check for right paren.
            $29 of                  \ ret-lst c-addr u str
                dup i < if
                    \ Add previous token.

                    \ Calc string start.
                    #2 pick         \ ret-lst c-addr u str c-addr
                    over +          \ ret-lst c-addr u str c-addr+

                    \ Calc len.
                    i               \ ret-lst c-addr u str c-addr+ i
                    #2 pick         \ ret-lst c-addr u str c-addr+ i str
                    -               \ ret-lst c-addr u str c-addr+ u

                    \ Make token.
                    token-new       \ ret-lst c-addr u str tkn

                    \ Store token.
                    #4 pick         \ ret-lst c-addr u str tkn ret-lst
                    list-push-end-struct
                then

                \ Add right paren token.
                s" )" token-new     \ ret-lst c-addr u str tkn
                #4 pick             \ ret-lst c-addr u str tkn ret-lst
                list-push-end-struct

                \ Set new start addr.
                drop i 1+
            endof
            \ Check for space.
            $20 of                  \ ret-lst c-addr u str
                dup i < if
                    \ Add previous token.

                    \ Calc string start.
                    #2 pick         \ ret-lst c-addr u str c-addr
                    over +          \ ret-lst c-addr u str c-addr+

                    \ Calc len.
                    i               \ ret-lst c-addr u str c-addr+ i
                    #2 pick         \ ret-lst c-addr u str c-addr+ i str
                    -               \ ret-lst c-addr u str c-addr+ u

                    \ Make token.
                    token-new       \ ret-lst c-addr u str tkn

                    \ Store token.
                    #4 pick         \ ret-lst c-addr u str tkn ret-lst
                    list-push-end-struct
                then

                \ Set new start addr.
                drop i 1+
            endof
            \ Check comma.
            $2C of                  \ ret-lst c-addr u str
                dup i < if
                    \ Add previous token.

                    \ Calc string start.
                    #2 pick         \ ret-lst c-addr u str c-addr
                    over +          \ ret-lst c-addr u str c-addr+

                    \ Calc len.
                    i               \ ret-lst c-addr u str c-addr+ i
                    #2 pick         \ ret-lst c-addr u str c-addr+ i str
                    -               \ ret-lst c-addr u str c-addr+ u

                    \ Make token.
                    token-new       \ ret-lst c-addr u str tkn

                    \ Store token.
                    #4 pick         \ ret-lst c-addr u str tkn ret-lst
                    list-push-end-struct
                then

                \ Set new start addr.
                drop i 1+
            endof
        endcase
    loop
                                    \ ret-lst c-addr u str

    \ Check for last token, if any.
    2dup -                          \ ret-lst c-addr u str diff
    if
        \ Add previous token.

        \ Calc string start.
        #2 pick                     \ ret-lst c-addr u str c-addr
        over +                      \ ret-lst c-addr u str c-addr+

        \ Calc len.
        #2 pick                     \ ret-lst c-addr u str c-addr+ u
        #2 pick                     \ ret-lst c-addr u str c-addr+ i str
        -                           \ ret-lst c-addr u str c-addr+ u

        \ Make token.
        token-new                   \ ret-lst c-addr u str tkn

       \ Store token.
       #4 pick                      \ ret-lst c-addr u str tkn ret-lst
       list-push-end-struct
    then

    2drop drop

    \ Check parens are balanced.

    \ Init counter.
    0                               \ ret-lst cnt

    \ Prep for loop.
    over list-get-links             \ ret-lst cnt link

    begin
        ?dup
    while
        \ Check for left paren.
        s" ("                       \ ret-lst cnt link c-addr u
        #2 pick link-get-data       \ ret-lst cnt link c-addr u tkt
        token-eq-string             \ ret-lst cnt link flag
        if
            \ Inc paren counter.
            swap 1+ swap
        then

        \ Check for right paren.
        s" )"                       \ ret-lst cnt link c-addr u
        #2 pick link-get-data       \ ret-lst cnt link c-addr u tkt
        token-eq-string             \ ret-lst cnt link flag
        if
            \ Dec paren counter.
            swap 1-                 \ ret-lst link cnt-

            \ Check counter not LT zero.
            dup 0 <                 \ ret-lst link cnt- flag
            if
                2drop
                token-list-deallocate
                false
                exit
            then

            swap                    \ ret-lst cnt- link
        then

        link-get-next
    repeat

    \ Check paren counter eq zero.  \ ret-lst cnt
    0= if
        true
    else
        token-list-deallocate
        false
    then
;
