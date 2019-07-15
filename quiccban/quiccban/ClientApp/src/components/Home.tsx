import * as React from 'react';
import Login from './Login';
import { RouteComponentProps } from 'react-router';
import { User } from '../App';
import { Typography } from '@material-ui/core';

export interface HomeProps extends RouteComponentProps {
    user?: User;
}

export default function Home(props: HomeProps) {

    if (!props.user)
        return <Login />;
    else
        return <Typography>Hi, {props.user.username}</Typography>;
}