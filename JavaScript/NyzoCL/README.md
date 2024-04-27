Ported from n-y-z-o's chrome extension repository with more encapsulation, ES2016 standard, and some extra comments or function overloading where deemed appropriate.




All properties, if they are intended to be accessible out of the scope of the class itself, are accessed through functions, this allows for more flexibility in regards to changes in definition or requirements.




Class functions start with a capital letter, private class functions and properties with an underscore and any other variable or function starts with a lowercase letter




The tweetnacl (nacl.min.js) file has been replaced by the npm package dependency of tweetnacl 1.0.0-rc.1, which is of the closest available commit hash height to the previous tweetnacl browser js file. This version was released on the same day & the extra commits on that day are limited to metadata and README edits. (https://github.com/dchest/tweetnacl-js/commits/88b8ea49b771f15d9e447bfc3eaba260bed2daff/). 




An attempt was made to install tweetnacl as a dependency by referencing the git repository and its commit hash, yet npm attempted to install its developer dependencies even when instructed not to do so, leading to errors. (e.g. npm ERR! npm WARN deprecated electron-download@3.0.1: Please use @electron/get moving forward. ; which lead to an electron checksum error later on)




The node_modules folder containing tweetnacl is not excluded from the repository and acts as a backup of the library. Developer dependencies have been omitted.