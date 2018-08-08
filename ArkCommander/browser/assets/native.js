function CallNative(type, data) {
    return eval("boundAsync." + type + "('" + encodeURIComponent(data) + "');");
}