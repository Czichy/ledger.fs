﻿module ParseTest

(*
/////////
run pTransactionFile "#foo\n\n  #Bar\nVERIFY-BALANCE  2015-01-02  account123 $123 AUD"
run pVerifyBalance "VERIFY-BALANCE  2015-01-02  account123 $123 AUD"
run pAmount "$123"
run pAmount " $123.00"
run pAmount "$123.00"
run pOptionalSpace ""
run pMandatorySpace "1 1"

run pOptionalSpace "1"
run pMandatorySpace "1"
run pYear "1985"       
run pMonth " 1985"       
run pDay " 1985"       
run pDate "1985-01-01"       
run pCommentLine"#123\n"
run pOptionalSpace ""
run pBlankLine ""
run pCommentLine"#123\n"
run pVerifyBalance ""
run pBlankLine"\n"
run pCommentLine"#123\n"
run pItem "#123\n"
run pItem "  \n#123\n"
run pItem " A#  \n#123\n"
run pCommentLine "#FooBarBaz"
run pBlankLine  "   \n#ABC\n\n#Def"
run pItems  "   \n#ABC\n\n#Def"
run pItem  "   \n#ABC\n\n#Def"
run pOptionalSpace  "   \n#ABC\n\n#Def"


run pTransactionFile  "#ABC\n\n#Def"

run pEol "\n#FooBarBaz"
run pCommentStart "#FooBarBaz"
run (restOfLine true) "ABC\n"
// run pSymbol " foo bar baz"





*)