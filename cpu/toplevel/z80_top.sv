//============================================================================
// Z80 Top level
//============================================================================
`timescale 100 ps/ 100 ps

`include "z80.svh"

module z80_top (z80_if.dut z80);

//----------------------------------------------------------------------------
// Include a list of top-level signal wires
//----------------------------------------------------------------------------
`include "globals.i"

// Define internal data bus partitions separated by data bus switches
wire [7:0] db0;         // Segment connecting data pins and IR
wire [7:0] db1;         // Segment with ALU
wire [7:0] db2;         // Segment with msb part of the register address-side interface

// Master hold clock signal may be requested by the delay or timing unit
logic hold_clk;
assign hold_clk = hold_clk_delay | hold_clk_timing;

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// Control block
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// Collect the PLA instruction decode prefix bitfield
logic [6:0] prefix;
assign prefix = { use_ixiy, ~use_ixiy, in_halt, in_alu, table_xx, table_cb, table_ed };

ir          instruction_reg ( .*, .db(db0[7:0]) );
pla_decode  pla_decode ( .* );
reset       reset_block ( .* );
sequencer   sequencer ( .* );
execute     execute ( .* );
interrupts  interrupts ( .* );
decode_state decode_state ( .* );
clk_delay   clk_delay ( .* );
pin_control pin_control ( .* );

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// ALU and ALU control, including the flags
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
alu_control alu_control ( .*, .db(db1[7:0]), .op543(db0[5:3]) );
alu_select  alu_select ( .* );
alu_flags   alu_flags ( .*, .db(db1[7:0]) );
alu         alu ( .*, .db(db2[7:0]), .bsel(db0[5:3]) );

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// Register file and register control
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
wire [7:0] db_hi_as;
wire [7:0] db_lo_as;

reg_file    reg_file ( .*, .db_hi_ds(db2[7:0]), .db_lo_ds(db1[7:0]), .db_hi_as(db_hi_as[7:0]), .db_lo_as(db_lo_as[7:0]) );
reg_control reg_control ( .* );

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// Address latch (with the incrementer) and address pins
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
address_latch address_latch ( .*, .abus({db_hi_as[7:0], db_lo_as[7:0]}) );
address_pins  address_pins ( .*, .A(z80.A[15:0]) );
 
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
// Data path within the CPU in various forms, ending with data pins
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
bus_switch bus_switch ( .* );
data_switch sw2 ( .sw_up_en(bus_sw_2u), .sw_down_en(bus_sw_2d), .db_up(db1[7:0]), .db_down(db2[7:0]) );

// Generators for 0x00 and 0xFF on the data (instruction) bus
bus_zero    bus_zero ( .* );
bus_ff      bus_ff ( .* );

data_switch sw1 ( .sw_up_en(bus_sw_1u), .sw_down_en(bus_sw_1d), .db_up(db0[7:0]), .db_down(db1[7:0]) );

// External data pins connecting to the interface pins
data_pins   data_pins ( .*, .db(db0[7:0]), .D(z80.D[7:0]) );

// External control pins connecting to the interface pins
control_pins_n control_pins ( .*,
    .nM1     (z80.nM1),
    .nMREQ   (z80.nMREQ),
    .nIORQ   (z80.nIORQ),
    .nRD     (z80.nRD),
    .nWR     (z80.nWR),
    .nRFSH   (z80.nRFSH),
    .nHALT   (z80.nHALT),
    .nWAIT   (z80.nWAIT),
    .nBUSACK (z80.nBUSACK),
    .nINT    (z80.nINT),
    .nNMI    (z80.nNMI),
    .nRESET  (z80.nRESET),
    .nBUSRQ  (z80.nBUSRQ),
    .CPUCLK  (z80.CLK)
 );

endmodule
