
let props = new Object();
let dotnet;

export function initDotnet(net)
{
    dotnet = net;
}
export function propval(pname,h){
    return props[h][pname];
}
export function propvalwin(pname) {
    return window[pname];
}
export function propref(pname, proph, h) {
    props[proph] = props[h][pname];
}
export function proprefwin(pname, proph) {
    props[proph] = window[pname];
}
export function deleteprop(phash) {
    delete props[phash];
}
export function isprop(pname, h) {
    return typeof (props[h][pname]) != "function" && typeof (props[h][pname]) != undefined;
}
export function ispropwin(pname) {
    return typeof (window[pname]) != "function" && typeof (window[pname]) != undefined;
}
export function isfunc(pname, h) {
    return typeof (props[h][pname]) == "function";
}
export function isfuncwin(pname) {
    return typeof (window[pname]) == "function";
}

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
    for (var i = 0; i < param.length; i++) 
        if (param[i].type == 1)
            res.push(props[param[i].value]);
        else
            res.push(param[i].value);
    return res;
}

export async function funcrefawaitwin(fname, params,eh, ph) {
    let er = "";
    try {
        props[ph] = await window[fname](...paramexpand(params));
    } catch (e)
    {
        er = e.toString();
    }

    dotnet.invokeMethodAsync("errorMessage", eh, er);
}
export async function funcvoidrefawaitwin(fname,params,eh) {
    let er = "";
    try {
        await window[fname](...paramexpand(params));
    } catch (e) {
        er = e.toString();
    }

    dotnet.invokeMethodAsync("errorMessage", eh, er);
}