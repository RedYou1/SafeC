class Test:
	u8 c
	union:
		char a
		i32 b

	Test(char a):
		this.c = 0
		this.a = a

	Test(i32 b):
		this.c = 1
		this.b = b

	void Print(this):
		if this.c == 0:
			print(this.a)
			print('\n')
		else:
			print(this.b)
			print('\n')

i32 main():
	Test t = new Test('A')
	t.Print()
	t = new Test(45i)
	t.Print()
	return 0i