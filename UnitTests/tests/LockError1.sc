class Lock:
	&i32 a

	Lock(&i32 a):
		this.a = a

i32 main():
	i32 a = 5i
	Lock lock = new Lock(a)
	print(a)
	return 0i