module.exports = class CommonUtil {

    // undefined is a built-in JavaScript keyword and can be overridden, this assures consistency
    static IsUndefined(value){
        return value === void(0);
    }
}