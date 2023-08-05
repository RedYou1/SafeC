#include <stdio.h>
#include <stdlib.h>
#include <string.h>
typedef struct INumber INumber;
typedef struct Container$int {
	int a;
}Container$int;
typedef enum Colors {
	Colors$blue,
	Colors$red,
	Colors$yellow,
	Colors$green,
}Colors;
void Container$int_test$Colors(Container$int this, Colors b) {
	printf("Container{");
	printf("%i", this.a);
	printf(", ");
	printf("%i", b);
	printf("}");
}
Container$int Container$int_Container_0$Colors(int a, Colors* b) {
	Container$int this;
	this.a = a;
	Container$int_test$Colors(this, *b);
	return this;
}
typedef struct Container$Colors {
	Colors a;
}Container$Colors;
void Container$Colors_test$int(Container$Colors this, int b) {
	printf("Container{");
	printf("%i", this.a);
	printf(", ");
	printf("%i", b);
	printf("}");
}
Container$Colors Container$Colors_Container_0$int(Colors a, int* b) {
	Container$Colors this;
	this.a = a;
	Container$Colors_test$int(this, *b);
	return this;
}
int main() {
	Colors _0 = Colors$blue;
	Container$int_Container_0$Colors(32, &_0);
	printf("\n");
	int _1 = 16;
	Container$Colors_Container_0$int(Colors$red, &_1);
	return 0;
}
