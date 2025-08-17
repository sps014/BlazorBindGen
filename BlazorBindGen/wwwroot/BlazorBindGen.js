let props = {};
let dotnet;

let callbackResult = {};
let cbCount = 0;

export function initDotnet(net) {
    dotnet = net;
}
export function CreateWin(h) {
    props[h] = window;
}
export let PropVal = (pname, h) => props[h][pname];


export function IsNullOrUndefined(h) {
    return props[h] === undefined || props[h] === null;
}
export function PropRef(pname, proph, h) {
    props[proph] = props[h][pname];
}
export function DeletePtr(phash) {
    delete props[phash];
}
export let IsProp = (pname, h) => typeof (props[h][pname]) != "function"
    && typeof (props[h][pname]) !== undefined;

export let IsFunc = (pname, h) => typeof (props[h][pname]) == "function";

export function PropSet(pname, val, h) {
    props[h][pname] = val;
}
export function PropSetRef(pname, ph, h) {
    props[h][pname] = props[ph];
}
export function Func(fname,params, h) {
    return props[h][fname](...paramexpand(params));
}
export function FuncRef(fname, params, ph, h) {
    props[ph] = props[h][fname](...paramexpand(params));
}
export function FuncVoid(fname, params, h) {
    props[h][fname](...paramexpand(params));
}
export async function FuncRefAwait(fname, params, eh, ph,h) {
    let er = "";
    try { props[ph] = await props[h][fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, null);
}
export async function FuncVoidAwait(fname, params, eh,h) {
    let er = "";
    try { await props[h][fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, null);
}
export async function FuncAwait(fname, params, eh, h){
    let er = "",v=null;
    try { v = await props[h][fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, v);
}
export async function ImportGen(module, eh) {
    let er = "";
    try { await import(module); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, null);
}
export async function ImportReturn(module, eh, h) {
    let er = "", v = null;
    try { props[h] = await import(module); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, null);
}
export function Construct(classname, param, eh, h) {
    props[eh] = new props[h][classname](...paramexpand(param));
}
export function SetArrayToRef(array, eh) {
    props[eh] = Blazor.platform.toUint8Array(array);
}
export function GetArrayRef(array, eh) {
    Blazor.platform.toUint8Array(array).set(props[eh]);
}
export function FastLength(h) {
    return props[h].byteLength;
}
export function SetCallback(pname, dotnet, h) {
    props[h][pname] = callbackHandler.bind(dotnet);
}

export let AsJson=(h)=>JSON.stringify(props[h]);
export let To = (h) => props[h];

function paramexpand(param) {
    let res = [];
    param.forEach((pm) => {
        let r;
        switch (pm.type) {
            case 1:
                r = props[pm.value];
                break;
            case 2:
                r = callbackHandler.bind(pm.value);
                break;
            default:
                r = pm.value;
                break;
        }
        res.push(r);
    });
    return res;
}
function callbackHandler() {
    let arg = [];
    for (let i = 0; i < arguments.length; i++) 
        arg.push(arguments[i]);
    let h = cbCount++;
    callbackResult[h] = arg;
    this.invokeMethodAsync("ExecuteInCSharp", h, arg.length);
}
export function CleanUpArgs(cbh, h) {
    props[h] = callbackResult[cbh];
    delete callbackResult[cbh];
}
export function isEqualRef(oh, h) {
    return props[oh] === props[h];
}
export function logPtr( h)
{
    console.log(props[h]);
}