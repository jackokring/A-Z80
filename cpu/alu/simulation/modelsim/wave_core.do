onerror {resume}
quietly WaveActivateNextPane {} 0
add wave -noupdate -radix hexadecimal /test_core/op1_sig
add wave -noupdate -radix hexadecimal /test_core/op2_sig
add wave -noupdate -color Gold -itemcolor Gold -radix hexadecimal /test_core/result_sig
add wave -noupdate -color Red -itemcolor Red /test_core/cy_out_sig
add wave -noupdate /test_core/cy_in_sig
add wave -noupdate /test_core/R_sig
add wave -noupdate /test_core/S_sig
add wave -noupdate /test_core/V_sig
add wave -noupdate /test_core/cy_out_D_sig
add wave -noupdate /test_core/cy_out_C_sig
add wave -noupdate /test_core/cy_out_B_sig
TreeUpdate [SetDefaultTree]
WaveRestoreCursors {{Cursor 1} {1600 ps} 0}
quietly wave cursor active 1
configure wave -namecolwidth 150
configure wave -valuecolwidth 100
configure wave -justifyvalue left
configure wave -signalnamewidth 0
configure wave -snapdistance 10
configure wave -datasetprefix 0
configure wave -rowmargin 4
configure wave -childrowmargin 2
configure wave -gridoffset 0
configure wave -gridperiod 1
configure wave -griddelta 40
configure wave -timeline 1
configure wave -timelineunits ps
update
WaveRestoreZoom {1100 ps} {2100 ps}
