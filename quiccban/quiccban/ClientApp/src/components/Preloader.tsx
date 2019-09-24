import * as React from 'react';
import { CircularProgress } from "@material-ui/core";
import { PreloaderProps } from '../props/PreloaderProps';

export default function Preloader(props: PreloaderProps) {


    return <CircularProgress className={props.iclass ? props.iclass : undefined} size={(props.size ? props.size : 180)} thickness={props.thickness ? props.thickness : 1.2} />
}