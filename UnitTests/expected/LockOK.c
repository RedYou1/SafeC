#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef struct INumber INumber;
typedef struct Lock {
	int* a;
}Lock;
Lock Lock_Lock_0(int* a) {
	Lock this;
	this.a = a;
	return this;
}
int main() {
	int a = 5;
	Lock lock = Lock_Lock_0(&a);
	printf("%i", lock.a);
	return 0;
}