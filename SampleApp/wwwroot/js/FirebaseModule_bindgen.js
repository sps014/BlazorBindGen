import {testFunc,testFunc2,testFunc3} from "FirebaseBinding";
export function testFuncBinder()
{
    testFunc();
}
export function testFunc2Binder()
{
    testFunc2();
}
export function testFunc3Binder()
{
    return testFunc3();
}
