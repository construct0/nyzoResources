// The function "sanitizeString" was not ported from n-y-z-o/nyzoChromeExtension due to not being used and not filtering any ascii character
// https://i.imgur.com/GypN9bM.png

class NyzoMicropayUtil {
    static IsValidTipAmount(){
        let tipAmountMicronyzos = NyzoConverter.GetAmountOfMicroNyzos(tipString);
        return tipAmountMicronyzos >= 2 && tipAmountMicronyzos <= 10 * NyzoConstants.GetMicroNyzosPerNyzo();
    }

    static IsValidMaximumMicropayAmount(micropayString){
        let maximumAmountMicronyzos = NyzoConverter.GetAmountOfMicroNyzos(micropayString);
        return maximumAmountMicronyzos >= 2 && maximumAmountMicronyzos <= 50 * NyzoConstants.GetMicroNyzosPerNyzo();
    }

    static IsValidMaximumAutomaticAmount(automaticString){
        let automaticAmountMicronyzos = NyzoConverter.GetAmountOfMicroNyzos(automaticString);
        return automaticAmountMicronyzos >= 1 && automaticAmountMicronyzos <= NyzoConstants.GetMicroNyzosPerNyzo();
    }

    static IsValidMaximumAutomaticAuthorization(valueString){
        let value = NyzoConverter.GetAmountOfMicroNyzos(valueString);
        return value >= 10 && value <= 100 * NyzoConstants.GetMicroNyzosPerNyzo();
    }

    static SubmitTransaction(timestamp, senderPrivateSeed, receiverIdentifier, microNyzosToSend, senderData, endpoint, callback){
        let transaction = new NyzoTransaction();

        transaction.SetTimestamp(timestamp);
        transaction.SetAmount(microNyzosToSend);
        transaction.SetRecipientIdentifier(receiverIdentifier);
        transaction.SetPreviousBlockHash(0);
        transaction.SetPreviousBlockHash(NyzoConstants.GetGenesisBlockHash());
        transaction.SetSenderData(senderData);

        transaction.Sign(senderPrivateSeed);
    
        return NyzoMicropayUtil.SubmitTransaction(transaction, endpoint, callback);
    }

    static SubmitTransaction(transaction, endpoint, callback){
        if(!(transaction instanceof NyzoTransaction)){
            console.error("[NyzoMicropayUtil][SubmitTransaction]: Failed to submit a transaction, provided instance is not a NyzoTransaction instance");
            callback(false, [], [], ["Failed to submit a transaction, provided instance is not a NyzoTransaction instance"], null);
            return false;
        }

        let httpRequest = new XMLHttpRequest();
        let transactionString = NyzoStringEncoder.NyzoStringFromTransaction(transaction);

        // Set up the event handler
        httpRequest.onreadystatechange = function () {
            // If the http request has been completed
            if(this.readyState == 4){
                let result = null;
                
                try {
                    result = JSON.parse(this.responseText);
                } catch (exception) {
                    errors = ["The response from the server was not valid"];
                }

                let success = false;
                let messages = null;
                let warnings = null;
                let errors = null;

                if (typeof result === "object" && result != null) {
                    // Store the warnings and errors
                    if (typeof result.errors === "object") {
                        errors = result.errors;
                    }

                    if (typeof result.notices === "object") {
                        warnings = result.notices;
                    }

                    // If the transaction was forwarded, indicate success
                    if (this.status === 200 && typeof result.result === "object" && typeof result.result[0] == "object") {
                        let resultFields = result.result[0];
                        
                        if (resultFields.forwarded === true && typeof resultFields.blockHeight === "number") {
                            success = true;
                            messages = [`The transaction was forwarded to the cycle for incorporation into block ${resultFields.blockHeight}`];
                        }
                    }
                }

                // Ensure some feedback is provided
                if (messages === null && warnings === null && errors === null) {
                    errors = ["The transaction failed to send"];
                }

                if (messages === null) {
                    messages = [];
                }

                if (warnings === null) {
                    warnings = [];
                }

                callback(success, messages, warnings, errors, transactionString);
                return success;
            }
        }

        // Perform the request 
        httpRequest.open("GET", `${endpoint}?transaction=${transactionString}`, true);
        httpRequest.send();
    }

    static CreateSupplementalTransaction(referentialTransaction, senderPrivateSeed){
        if(!(referentialTransaction instanceof NyzoTransaction)){
            console.error("[NyzoMicropayUtil][CreateSupplementalTransaction]: Failed to create a supplemental transaction, provided referential transaction is not a NyzoTransaction instance");
            return null;
        }

        const transaction = new NyzoTransaction();
        transaction.SetTimestamp(Date.now());
        transaction.SetAmount(1);
        transaction.SetRecipientIdentifier(referentialTransaction.recipientIdentifier);
        transaction.SetPreviousHashHeight(0);
        transaction.SetPreviousBlockHash(NyzoConstants.GetGenesisBlockHash());
        transaction.SetSenderData(referentialTransaction.senderData);

        transaction.Sign(senderPrivateSeed);
    
        return transaction;
    }
}