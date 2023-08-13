Result<i32, str> test():
	return Ok<i32, str>(1)

Result<i32, str> test2():
	return Error<i32, str>("NON")

i32 main():
	Result<i32, str> a = test()
	if a.IsOk():
		print(a.Ok())
		print('\n')
	else:
		print(a.Error())
		print('\n')

	a = test2()
	bool t = a.IsError()
	if t:
		print(a.Error())
		print('\n')
	else:
		print(a.Ok())
		print('\n')
		
	return 0i