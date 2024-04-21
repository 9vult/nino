
export const interp = (input: string, swaps: {[key:string]:any}) => {
  for (let key in Object.keys(swaps)) {
    input.replaceAll(key, swaps[key]);
  }
  return input;
};