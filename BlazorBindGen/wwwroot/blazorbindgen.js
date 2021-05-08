
let props = new Object();

export function propval(pname,h)
{
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