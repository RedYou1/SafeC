#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef struct INumber INumber;
typedef struct Test {
	unsigned char c;
	union {
		char a;
		int b;
	};
}Test;
Test Test_Test_0(char a) {
	Test this;
	this.c = 0;
	this.a = a;
	return this;
}
void Test_Print(Test this) {
	if (this.c == 0) {
		printf("%c", this.a);
		printf("%c", '\n');
	}
	else {
		printf("%i", this.b);
		printf("%c", '\n');
	}
}
Test Test_Test_1(int b) {
	Test this;
	this.c = 1;
	this.b = b;
	return this;
}
int main() {
	Test t = Test_Test_0('A');
	Test_Print(t);
	t = Test_Test_1(45);
	Test_Print(t);
	return 0;
}
