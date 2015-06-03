# Introduction #

The R4iSaveDongle is a save dumper sold by the R4i-sdhc.com website.
It is a new save dumper which uses the HID compliance so it doesn't need
installing.

# Protocol #

The protocol is very simple: (these are only sample instruction and not the actual ones)

these are HID commands each has a return packets amount (or none)

each packet is 0x40 bytes long.

  * 0x111103 - NOACK - start reading
  * 0x222200 - AK\*10 - get eeprom's header - for 3ds returns the  flash\_id
  * 0x2F2F03 - NOACK - stop reading
  * 0x333303 - ACK\*8 - read 512 bytes (followed by a 24bit offset Big endian)
  * 0x444400 - NOACK - write 32 bytes (followed by a 24bit offset Big endian)
  * 0x666603 - NOACK - ?? clear ram.
  * 0xA00000 - ACK\*3 - ?? check version

Reading and writing is made by first stopping what ever process is working with 0x2f2f
  * and then cleaning with 0x6666
  * then start with 0x1111
  * repeat the action wanted read/write
  * and then stop with 0x2f2f