module.exports = class NyzoMicropayConfiguration {
    constructor(clientUrl, receiverId, tag, displayName, amountMicroNyzos) {
        let cleanTag = (_tag) => {
            // This was previously called cleanTag in the extensionUtil

            // Previously, the tag was limited to 32 characters and non-word characters were removed. Now, the tag will be
            // limited to 67 characters to provide support for normalized sender-data strings, and all characters are allowed.
            // This allows new functionality without breaking previous functionality.
            if (typeof _tag !== "string") {
                _tag = "";
            }

            return _tag.substring(0, 67);
        }

        let cleanDisplayName = (_displayName) => {
            if (typeof _displayName !== "string") {
                _displayName = "";
            }
            return _displayName.replace(/[^\w_ ]/g, "");
        }

        this.tag = cleanTag(tag);
        this.displayName = cleanDisplayName(displayName);
        this.clientUrl = NyzoUtil.IsValidClientURL(clientUrl) ? clientUrl : null;
        this.receiverId = NyzoUtil.IsValidPublicIdentifier(receiverId) ? receiverId : null;
        this.amountMicronyzos = amountMicroNyzos;
    }

    static GetClientURL(){
        return this.clientUrl;
    }

    static GetReceiverID(){
        return this.receiverId;
    }

    static GetTag(){
        return this.tag;
    }

    static GetDisplayName(){
        return this.displayName;
    }

    static GetAmountOfMicroNyzos(){
        return this.amountMicronyzos;
    }

    static GetAmountOfNyzos(){
        return Math.floor(this.amountMicronyzos / NyzoConstants.GetMicroNyzosPerNyzo());
    }
}