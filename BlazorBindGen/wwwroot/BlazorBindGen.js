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
let conv_str=(s) => BINDING.conv_string(s);
export let PropVal = (pname, h) => props[h][pname];
export let propvalwin = (pname) => window[pname];
export function propref(pname, proph, h) {
    props[proph] = props[h][conv_str(pname)];
}
export function proprefgen(pname, proph, h) {
    props[proph] = props[h][pname];
}
export function DeletePtr(phash) {
    delete props[phash];
}
export let isprop = (pname, h) => typeof (props[h][conv_str(pname)]) != "function"
    && typeof (props[h][conv_str(pname)]) !== undefined;
export let ispropgen = (pname, h) => typeof (props[h][pname]) != "function"
    && typeof (props[h][pname]) !== undefined;

export let isfunc = (pname, h) => typeof (props[h][conv_str(pname)]) == "function";
export let isfuncgen = (pname, h) => typeof (props[h][pname]) == "function";

export function propset(pname, val, h) {
    props[h][pname] = val;
}
export function propsetref(pname, ph, h) {
    props[h][conv_str(pname)] = props[ph];
}
export function propsetrefgen(pname, ph, h) {
    props[h][pname] = props[ph];
}
export function func(fname,params, h) {
    return props[h][fname](...paramexpand(params));
}
export function funcref(fname, params, ph, h) {
    props[ph] = props[h][fname](...paramexpand(params));
}
export function funcvoid(fname, params, h) {
    props[h][fname](...paramexpand(params));
}
export async function funcrefawait(fname, params, eh, ph,h) {
    let er = "";
    try { props[ph] = await props[h][fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, null);
}
export async function funcvoidawait(fname, params, eh,h) {
    let er = "";
    try { await props[h][fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, null);
}
export async function funcawait(fname, params, eh, h){
    let er = "",v=null;
    try { v = await props[h][fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, v);
}
export async function ImportWasm(module, eh) {
    let er = "";
    try { await import(conv_str(module)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, null);
}
export async function ImportGen(module, eh) {
    let er = "";
    try { await import(module); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethodAsync("errorMessage", eh, er, null);
}
export function construct(classname, param, eh, h) {
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
export function setcallback(pname, dotnet, h) {
    props[h][pname] = callbackHandler.bind(dotnet);
}

export let asjson=(h)=>JSON.stringify(props[h]);
export let to = (h) => props[h];

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
