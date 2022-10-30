

# make file for simulated emulated equipment
#

CC=gcc
CFLAGS=-I.

build: N6700c.c
	$(CC) N6700c.c -o N6700c.o 

clean:
	rm *.o

execute: build
	./N6700c.o 1234



