#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef struct INumber INumber;
typedef struct Result$i32$str {
	char o;
	union {
		int r;
		char* e;
	};
}Result$i32$str;
Result$i32$str Ok$i32$str(int r) {
	Result$i32$str this;
	this.o = 1;
	this.r = r;
	return this;
}
Result$i32$str test() {
	return Ok$i32$str(1);
}
char IsOk(Result$i32$str this) {
	return this.o;
}
int Ok(Result$i32$str this) {
	return this.r;
}
char* Error(Result$i32$str this) {
	return this.e;
}
Result$i32$str Error$i32$str(char* e) {
	Result$i32$str this;
	this.o = 0;
	this.e = e;
	return this;
}
Result$i32$str test2() {
	return Error$i32$str("NON");
}
char IsError(Result$i32$str this) {
	return !this.o;
}
int main() {
	Result$i32$str a = test();
	if (IsOk(a)) {
		printf("%i", Ok(a));
		printf("%c", '\n');
	}
	else {
		printf(Error(a));
		printf("%c", '\n');
	}
	a = test2();
	char t = IsError(a);
	if (t) {
		printf(Error(a));
		printf("%c", '\n');
	}
	else {
		printf("%i", Ok(a));
		printf("%c", '\n');
	}
	return 0;
}
