import React from "react";
import Dialog from "@material-ui/core/Dialog";
import Button from "@material-ui/core/Button";
import DialogTitle from "@material-ui/core/DialogTitle";
import DialogContent from "@material-ui/core/DialogContent";
import Paper from "@material-ui/core/Paper";
import { makeStyles, fade } from "@material-ui/core/styles";
import Tabs from "@material-ui/core/Tabs";
import Tab from "@material-ui/core/Tab";
import PersonPinIcon from "@material-ui/icons/PersonPin";
import LockOpen from "@material-ui/icons/LockOpen";
import Typography from "@material-ui/core/Typography";
import PropTypes from "prop-types";
import LoginWrapper from "../../containers/login";
import RegisterWrapper from "../../containers/register";
import register from "../../registerServiceWorker";
import reset from '../../containers/header-profile'
import { connect } from 'react-redux';
import { TogleOpenWind } from '../../actions/modalWind';

function TabContainer(props) {
  return (
    <Typography component="div" style={{ padding: 8 * 3 }}>
      {props.children}
    </Typography>
  );
}

const useStyles = makeStyles({
  root: {
    flexGrow: 1,
    maxWidth: 500
  }
});

function ModalWind(props) {
  const classes = useStyles();
  const [value, setValue] = React.useState(0);
    
  const handleChange = (event, newValue) => { 
    setValue(newValue); 
  }

  const handleClickOpen = () => { 
    props.setStatus(true); 
  }
  
  const handleClose = () => { 
    props.setStatus(false); 
    props.reset();
  }

  return (
    <div className='d-inline-block'>
      <Button className='mt-5' variant="outlined" color="primary" onClick={handleClickOpen}>
        Sign In/Up
      </Button>
      <Dialog
        open={props.status.isOpen}
        onClose={handleClose}
      >
        <Paper square className={classes.root}>
          <Tabs
            value={value}
            onChange={handleChange}
            variant="fullWidth"
            indicatorColor="primary"
            textColor="primary"
          >
            <Tab icon={<LockOpen />} label="Login" />
            <Tab icon={<PersonPinIcon />} label="Register" />
          </Tabs>
          
          {value === 0 && (
            <TabContainer>
              <LoginWrapper  />
            </TabContainer>
          )}
          {value === 1 && (
            <TabContainer>
              <RegisterWrapper handleClose={handleClose} />
            </TabContainer>
          )}

          <Button fullWidth onClick={handleClose} color="primary">
            Cancel
          </Button>
        </Paper>
      </Dialog>
    </div>
)}

const mapStateToProps = (state) => ({
  status: state.modal
});

const mapDispatchToProps = (dispatch) => ({
  setStatus: (data) => dispatch(TogleOpenWind(data))
});

export default connect(mapStateToProps, mapDispatchToProps)(ModalWind)