module.exports = class NyzoConstants {
    static GetGenesisBlockHash(){
        return NyzoUtil.HexStringAsUint8Array("bc4cca2a2a50a229-256ae3f5b2b5cd49-aa1df1e2d0192726-c4bb41cdcea15364");
    }

    static GetMicroNyzosPerNyzo(){
        return 1000000;
    }

    static GetMaximumSenderDataLength(){ 
        return 32;
    }

    static GetMinimumTransactionAmount(){
        let microNyzosPerNyzo = NyzoConstants.GetMicroNyzosPerNyzo().toString();
        let length = microNyzosPerNyzo.length - 1;
        let result = "0.";

        for(let i=1; i <= length; i++){
            result += ((i == length) ? "1" : "0");
        }

        return parseFloat(result);
    }
}