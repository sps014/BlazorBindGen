
let props = new Object();
let dotnet;
export function initDotnet(net) {
    dotnet = net;
}
export let propval = (pname, h) => props[h][pname];
export let propvalwin=(pname)=> window[pname];
export function propref(pname, proph, h) {
    props[proph] = props[h][pname];
}
export function proprefwin(pname, proph) {
    props[proph] = window[pname];
}
export function deleteprop(phash) {
    delete props[phash];
}
export let isprop = (pname, h) => typeof (props[h][pname]) != "function" && typeof (props[h][pname]) != undefined;
export let ispropwin = (pname) => typeof (window[pname]) != "function" && typeof (window[pname]) != undefined;
export let isfunc=(pname, h)=> typeof (props[h][pname]) == "function";
export let isfuncwin = (pname) => typeof (window[pname]) == "function";
export function propsetwin(pname, val) {
    window[pname] = val;
}
export function propsetrefwin(pname, h) {
    window[pname] = props[h];
}
export function propset(pname, val, h) {
    props[h][pname] = val;
}
export function propsetref(pname, ph, h) {
    props[h][pname] = props[ph];
}
export function func(fname,params, h) {
    return props[h][fname](...paramexpand(params));
}
export function funcwin(fname, params) {
    return window[fname](...paramexpand(params));
}
export function funcref(fname, params, ph, h) {
    props[ph] = props[h][fname](...paramexpand(params));
}
export function funcrefwin(fname, params, ph) {
    props[ph] = window[fname](...paramexpand(params));
}
export function funcvoid(fname, params, h) {
    props[h][fname](...paramexpand(params));
}
export function funcvoidwin(fname, params) {
    window[fname](...paramexpand(params));
}
function paramexpand(param) {
    var res = [];
    param.forEach((pm) => {
        let r;
        switch (pm.type) {
            case 1:
                r = props[pm.value];
                break;
            case 2:
                r = callme.bind(pm.value);
                break;
            default:
                r = pm.value;
                break;
        }
        res.push(r);
    });
    return res;
}
function callme() {
    let arg = [];
    for (var i = 0; i < arguments.length; i++) {
        arg.push(arguments[i]);
    }
    this.invokeMethod("ExecuteInCSharp", arg);
}
export async function funcrefawaitwin(fname, params,eh, ph) {
    let er = "";
    try { props[ph] = await window[fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethod("errorMessage", eh, er, null);
}
export async function funcvoidawaitwin(fname,params,eh) {
    let er = "";
    try { await window[fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethod("errorMessage", eh, er, null);
}
export async function funcawaitwin(fname, params, eh) {
    let er = "", v = null;
    try { v = await window[fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethod("errorMessage", eh, er, v);
}
export async function funcrefawait(fname, params, eh, ph,h) {
    let er = "";
    try { props[ph] = await props[h][fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethod("errorMessage", eh, er, null);
}
export async function funcvoidawait(fname, params, eh,h) {
    let er = "";
    try { await props[h][fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethod("errorMessage", eh, er, null);
}
export async function funcawait(fname, params, eh, h){
    let er = "",v=null;
    try { v = await props[h][fname](...paramexpand(params)); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethod("errorMessage", eh, er, v);
}
export async function importmod(module, eh) {
    let er = "";
    try { await import(module); }
    catch (e) { er = e.toString(); }
    dotnet.invokeMethod("errorMessage", eh, er, null);
}
export function constructwin(classname, param, h) {
    props[h] = new window[classname](...paramexpand(param));
}
export function construct(classname, param, eh, h) {
    props[eh] = new props[h][classname](...paramexpand(param));
}
export let asjson=(h)=>JSON.stringify(props[h]);
export let to = (h) => props[h];
