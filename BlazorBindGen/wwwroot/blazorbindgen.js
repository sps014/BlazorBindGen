
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