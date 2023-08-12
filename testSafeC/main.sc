enum Colors:
	blue
	red
	yellow
	green

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

class A:
	i32 a

	A():
		this.a = 1i

	A(i32 a):
		this.a = a

	void Add(*dyn this):
		this.a = this.a + 1

	void Add2(*dyn this, i32 b):
		this.a = this.a + b

class B:
	str b
	Colors color

	B(Colors color):
		this.b = "Allo"
		this.color = color

	void Print(*dyn this?):
		print("Hello World {")
		if this is null:
			print("null")
		else:
			print(this.b)
			print(", ")
			print(this.color)
		print("}\n")

class C(A):
	str id
	B b

	C(str id):
		base()
		this.id = id
		this.b = new B(Colors.green)

	C(i32 a, str id):
		base(a)
		this.id = id
		this.b = new B(Colors.yellow)

	void Add(*dyn this):
		this.a = this.a + 2

class Container<T>:
	T a

	Container<V>(T a, *V b):
		this.a = a
		this.test<V>(b)

	void test<V>(this, V b):
		print("Container{")
		print(this.a)
		print(", ")
		print(b)
		print("}")

B createB(Colors color):
	B b = new B(color)
	return b

C createC(i32? a, str id):
	if a is not null:
		return new C(a, id)
	return new C(id)

void Add(*typedyn A a):
	a.Add()
	if a is C c:
		B b = c.b
		b.Print()
		print("add is type C\n")
	else:
		print("add is not type C\n")

void TakeOwner(A a):
	print(a.a)
	print("\n")

class Lock:
	i32 l
	&typedyn A a

	Lock(i32 l, &typedyn A a):
		this.l = l
		this.a = a
		Add(this.a)

i32 main():
	A aa = new A()
	print(aa.a)
	print("\n")
	Add(aa)
	TakeOwner(aa)

	C a = createC(null, "ID")
	print(a.id)
	print(":\n")
	print(a.a)
	print("\n")
	Add(a)
	print(a.a)
	print("\n")
	
	a = createC(5i, "ID2")
	a = createC(6i, "ID2,1")
	print(a.id)
	print(":\n")
	print(a.a)
	print("\n")
	a.Add2(5i)
	print(a.a)
	print("\n")

	B? b = null
	b.Print()
	b = createB(Colors.blue)
	b.Print()
	b = createB(Colors.red)
	b.Print()


	bool start = a.id.StartsWith("ID2")

	print(start)
	print("\n")

	a.Add2(5i)

	u64 sl = a.id.Len()
	print(sl)
	print("\n")

	Lock lock = new Lock(5i, a)

	new<Colors> Container<i32>(32i, Colors.blue)
	print("\n")
	new<i32> Container<Colors>(Colors.red, 16i)
	print('\n')

	Test t = new Test('A')
	t.Print()
	t = new Test(45i)
	t.Print()
	return 0i