
let props = new Object();

export function print_prop(str)
{
    console.log(window[str]);
}
export function prop(pname,h)
{
    return props[h][pname];
}
export function propwin(pname) {
    return window[pname];
}
export function propref(pname, proph, h) {
    props[proph] = props[h][pname];
}
export function proprefwin(pname, proph) {
    props[proph] = window[pname];
}