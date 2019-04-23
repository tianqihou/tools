@115
.text
.thumb
.align 1

预知未来:
	jumpiftype 0x19 0x0 0x29034391
	accuracycheck 0x1d2b26d0 0xffd1
	activesidesomething
	maxattackhalvehp 0x52101ff
	attackcanceler
	jumpifstat bank_target 0xff 0xf7 0xf3 0x1c2301fc
	attackcanceler
	cmd68
	goto_cmd 0x2400d007
0x29034391:

0x1c2301fc:

0x2400d007:
